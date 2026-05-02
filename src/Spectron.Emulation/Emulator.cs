using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.DivMmc;
using OldBit.Spectron.Emulation.Devices.DivMmc.RTC;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Interface1;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Devices.Printer;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Rzx;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Emulation.TimeTravel;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation;

/// <summary>
/// This is the main emulator class that ties everything together.
/// </summary>
public sealed class Emulator
{
    private readonly HardwareSettings _hardware;
    private readonly TimeMachine _timeMachine;
    private readonly ILogger _logger;
    private readonly EmulatorTimer _emulationTimer;
    private readonly FloatingBus _floatingBus;
    private readonly ScreenMemoryHandler _screenMemoryHandler;

    private bool _isDebuggerResume;
    private bool _invalidateScreen;
    private bool _isAcceleratedTapeSpeed;
    private bool _isNmiRequested;
    private bool _isDebuggerBreak;
    private long _ticksSinceReset;

    public delegate void FrameEvent(FrameBuffer frameBuffer, AudioBuffer audioBuffer);
    public event FrameEvent? FrameCompleted;

    public bool IsPaused => _emulationTimer.IsPaused;

    public bool IsUlaPlusEnabled
    {
        get => UlaPlus.IsEnabled;
        set => ToggleUlaPlus(value);
    }

    public bool IsFloatingBusEnabled
    {
        set => _floatingBus.IsEnabled = value;
    }

    public KeyboardState KeyboardState { get; }
    public TapeManager TapeManager { get; }
    public MicrodriveManager MicrodriveManager { get; }
    public DiskDriveManager DiskDriveManager { get; }
    public JoystickManager JoystickManager { get; }
    public AudioManager AudioManager { get; }
    public GamepadManager GamepadManager { get; }
    public MouseManager MouseManager { get; }
    public CommandManager CommandManager { get; }
    public RzxController? RzxController { get; internal set; }

    public ComputerType ComputerType { get; }
    public RomType RomType { get; }

    public Z80 Cpu { get; }
    internal Ula Ula { get; }
    public IEmulatorMemory Memory { get; }
    public SpectrumBus Bus { get; }
    public DivMmcDevice DivMmc { get; }
    public Beta128Device Beta128 { get; }
    public Interface1Device Interface1 { get; }
    public ZxPrinter Printer { get; }
    public ScreenBuffer ScreenBuffer { get; }

    public int TicksPerFrame => _hardware.TicksPerFrame;

    internal UlaPlus UlaPlus { get; }

    internal Emulator(
        EmulatorArgs emulatorArgs,
        HardwareSettings hardware,
        TapeManager tapeManager,
        MicrodriveManager microdriveManager,
        DiskDriveManager diskDriveManager,
        GamepadManager gamepadManager,
        KeyboardState keyboardState,
        TimeMachine timeMachine,
        CommandManager commandManager,
        ILogger logger)
    {
        _hardware = hardware;
        KeyboardState = keyboardState;
        _timeMachine = timeMachine;
        _logger = logger;

        CommandManager = commandManager;
        TapeManager = tapeManager;
        MicrodriveManager = microdriveManager;
        DiskDriveManager = diskDriveManager;
        GamepadManager = gamepadManager;
        ComputerType = emulatorArgs.ComputerType;
        RomType = emulatorArgs.RomType;
        Memory = emulatorArgs.Memory;

        Cpu = new Z80(emulatorArgs.Memory)
        {
            Clock =
            {
                InterruptDuration = hardware.InterruptDuration,
                ContentionProvider = emulatorArgs.ContentionProvider
            }
        };

        UlaPlus = new UlaPlus();
        ScreenBuffer = new ScreenBuffer(hardware, emulatorArgs.Memory, UlaPlus);

        Ula = ComputerType == ComputerType.Timex2048
            ? new UlaTimex(KeyboardState, ScreenBuffer, Cpu, TapeManager)
            : new Ula(KeyboardState, ScreenBuffer, Cpu, TapeManager);

        _screenMemoryHandler = new ScreenMemoryHandler(Memory, ScreenBuffer);
        _screenMemoryHandler.SetScreenMode(Ula as UlaTimex);

        Bus = new SpectrumBus();

        JoystickManager = new JoystickManager(gamepadManager, Bus, KeyboardState);
        MouseManager = new MouseManager(Bus);
        KeyboardState.Reset();
        TapeManager.Attach(Cpu, Memory, hardware);

        _floatingBus = new FloatingBus(_hardware, Memory, Cpu.Clock, Ula.IsUlaPort);

        AudioManager = new AudioManager(Cpu.Clock, tapeManager.CassettePlayer, hardware, Ula.IsUlaPort);

        DivMmc = new DivMmcDevice(Cpu, Memory, logger);
        Beta128 = new Beta128Device(Cpu, _hardware.ClockMhz, Memory, ComputerType, diskDriveManager);
        Interface1 = microdriveManager.CreateDevice(Cpu, Memory);
        Printer = new ZxPrinter();

        AddDevices();
        AddEventHandlers();

        _emulationTimer = new EmulatorTimer();
        _emulationTimer.Elapsed += OnTimerElapsed;
    }

    public void Start()
    {
        AudioManager.Start();
        _emulationTimer.Start();
    }

    public void Shutdown(bool isAppClosing = false)
    {
        AudioManager.Stop();
        _emulationTimer.Stop();
        _emulationTimer.Dispose();

        if (isAppClosing)
        {
            GamepadManager.Stop();
        }

        JoystickManager.Stop();
        DivMmc.Stop();

        while (!_emulationTimer.IsStopped)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(50));
        }
    }

    public void Pause()
    {
        _emulationTimer.Pause();

        // Ensure that the emulator finishes the current frame before pausing
        Thread.Sleep(75);

        _timeMachine.AddEntry(this);
    }

    public void Resume(bool isDebuggerResume = false)
    {
        _isDebuggerBreak = false;
        _isDebuggerResume = isDebuggerResume;

        _emulationTimer.Resume();
    }

    public void Reset()
    {
        _isDebuggerBreak = false;
        _ticksSinceReset = 0;

        RzxController = null;
        AudioManager.ResetAudio();
        Memory.Reset();
        Cpu.Reset();
        ScreenBuffer.Reset();
        UlaPlus.Reset();
        KeyboardState.Reset();
        Interface1.Reset();
        DivMmc.Reset();
        Beta128.Reset();
        Ula.Reset();
    }

    public void Break()
    {
        Cpu.Break();

        _isDebuggerBreak = true;
    }

    public void RequestNmi() => _isNmiRequested = true;

    public void SetEmulationSpeed(int emulationSpeedPercentage) =>
        _emulationTimer.Interval = emulationSpeedPercentage == int.MaxValue ?
            TimeSpan.Zero :
            TimeSpan.FromMilliseconds(20 * (100f / emulationSpeedPercentage));

    private void OnTimerElapsed(object? sender, EventArgs e)
    {
        if (_isDebuggerBreak)
        {
            return;
        }

        try
        {
            RunFrame();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during frame execution");
        }
    }

    private void AddEventHandlers()
    {
        Cpu.Clock.TicksAdded += (_, previousFrameTicks, _) => ScreenBuffer.UpdateScreen(previousFrameTicks);
        Cpu.BeforeInstruction += BeforeInstruction;
        UlaPlus.ActiveChanged += _ => _invalidateScreen = true;
        Beta128.DiskActivity += _ => DiskDriveManager.OnDiskActivity();

        if (Ula is UlaTimex ulaTimex)
        {
            ulaTimex.ScreenModeChanged += (sender, _) =>
                _screenMemoryHandler.SetScreenMode(sender as UlaTimex, Cpu.Clock.FrameTicks);
        }
    }

    private void AddDevices()
    {
        Bus.AddDevice(Ula);
        Bus.AddDevice(UlaPlus);
        Bus.AddDevice(Memory);
        Bus.AddDevice(AudioManager.Beeper);
        Bus.AddDevice(AudioManager.Ay);
        Bus.AddDevice(Printer);
        Bus.AddDevice(Interface1);
        Bus.AddDevice(DivMmc);
        Bus.AddDevice(Beta128);
        Bus.AddDevice(new RtcDevice(DivMmc));
        Bus.AddDevice(_floatingBus);

        Cpu.AddBus(Bus);
    }

    internal void RunFrame()
    {
        StartFrame();

        if (_isNmiRequested)
        {
            _isNmiRequested = false;

            Cpu.TriggerNmi();
        }

        Cpu.Run();

        EndFrame();

        if (!_invalidateScreen)
        {
            return;
        }

        ScreenBuffer.Invalidate();
        _invalidateScreen = false;
    }

    private void StartFrame()
    {
        if (_isDebuggerResume)
        {
            _isDebuggerResume = false;

            return;
        }

        if ((RzxController?.IsPlaybackActive == true))
        {
            Cpu.Clock.NewFrame(frameFetches: RzxController.CurrentFrame?.FetchCounter ?? 0);
        }
        else
        {
            Cpu.Clock.NewFrame(_hardware.TicksPerFrame);
        }

        ScreenBuffer.NewFrame();
        AudioManager.NewFrame();
        Beta128.NewFrame(_ticksSinceReset);
    }

    private void EndFrame()
    {
        _ticksSinceReset += Cpu.Clock.FrameTicks;

        var audioBuffer = AudioManager.EndFrame();

        ScreenBuffer.EndFrame(Cpu.Clock.FrameTicks);

        FrameCompleted?.Invoke(ScreenBuffer.FrameBuffer, audioBuffer);

        if (RzxController?.IsPlaybackActive != true)
        {
            _timeMachine.AddEntry(this);
        }
        else
        {
            RzxController.NextFrame();
        }
    }

    private void ToggleUlaPlus(bool value)
    {
        UlaPlus.IsEnabled = value;
        _invalidateScreen = true;
    }

    private void BeforeInstruction(Word pc)
    {
        switch (pc)
        {
            case RomRoutines.LD_START:
                switch (TapeManager.TapeLoadSpeed)
                {
                    case TapeSpeed.Instant:
                        TapeManager.FastLoad();
                        break;

                    case TapeSpeed.Accelerated:
                        SetEmulationSpeed(int.MaxValue);
                        _isAcceleratedTapeSpeed = true;
                        break;
                }
                break;

            case RomRoutines.SA_BYTES:
                switch (TapeManager.TapeSaveSpeed)
                {
                    case TapeSpeed.Instant:
                        TapeManager.FastSave();
                        break;

                    case TapeSpeed.Accelerated:
                        SetEmulationSpeed(int.MaxValue);
                        _isAcceleratedTapeSpeed = true;
                        break;
                }

                break;

            case RomRoutines.LD_BYTES_RET:
            case RomRoutines.SA_BYTES_RET:
            case RomRoutines.ERROR_1:
                if (_isAcceleratedTapeSpeed)
                {
                    SetEmulationSpeed(100);
                    _isAcceleratedTapeSpeed = false;
                }

                break;

            // case 0x1AF1:
            // case RomRoutines.SAVE_ETC:
            //     SnapshotManager.Save("2048.szx", this);
            //     break;
        }
    }
}
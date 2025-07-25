using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.DivMmc;
using OldBit.Spectron.Emulation.Devices.DivMmc.RTC;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Devices.Printer;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape;
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
    private readonly SpectrumBus _spectrumBus;
    private readonly EmulatorTimer _emulationTimer;
    private readonly IEmulatorMemory _memory;

    private bool _isDebuggerResume;
    private bool _invalidateScreen;
    private bool _isAcceleratedTapeSpeed;
    private bool _isNmiRequested;
    private bool _isDebuggerBreak;
    private FloatingBus _floatingBus = null!;

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
    public JoystickManager JoystickManager { get; }
    public AudioManager AudioManager { get; }
    public GamepadManager GamepadManager { get; }
    public MouseManager MouseManager { get; }
    public CommandManager CommandManager { get; }

    public ComputerType ComputerType { get; }
    public RomType RomType { get; }
    public TapeSpeed TapeLoadSpeed { get; set; }

    public Z80 Cpu { get; }
    public IEmulatorMemory Memory => _memory;
    public IBus Bus => _spectrumBus;
    public DivMmcDevice DivMmc { get; }
    public ZxPrinter Printer { get; }

    public int TicksPerFrame => _hardware.TicksPerFrame;

    internal ScreenBuffer ScreenBuffer { get; }
    internal UlaPlus UlaPlus { get; }

    internal Emulator(
        EmulatorArgs emulatorArgs,
        HardwareSettings hardware,
        TapeManager tapeManager,
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
        GamepadManager = gamepadManager;
        ComputerType = emulatorArgs.ComputerType;
        RomType = emulatorArgs.RomType;
        _memory = emulatorArgs.Memory;

        UlaPlus = new UlaPlus();
        _spectrumBus = new SpectrumBus();
        ScreenBuffer = new ScreenBuffer(hardware, emulatorArgs.Memory, UlaPlus);
        Cpu = new Z80(emulatorArgs.Memory)
        {
            Clock =
            {
                InterruptDuration = hardware.InterruptDuration,
                ContentionProvider = emulatorArgs.ContentionProvider
            }
        };

        JoystickManager = new JoystickManager(gamepadManager, _spectrumBus, KeyboardState);
        MouseManager = new MouseManager(_spectrumBus);
        KeyboardState.Reset();
        TapeManager.Attach(Cpu, Memory, hardware);

        AudioManager = new AudioManager(Cpu.Clock, tapeManager.CassettePlayer, hardware);

        DivMmc = new DivMmcDevice(Cpu, _memory, logger);

        Printer = new ZxPrinter();

        SetupUlaAndDevices();
        SetupEventHandlers();

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

        AudioManager.ResetAudio();
        _memory.Reset();
        Cpu.Reset();
        ScreenBuffer.Reset();
        UlaPlus.Reset();
        KeyboardState.Reset();
        DivMmc.Reset();

        if (IsPaused)
        {
            Resume();
        }
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

    private void OnTimerElapsed(EventArgs e)
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

    private void SetupEventHandlers()
    {
        _memory.MemoryUpdated += address =>
        {
            if (address < 0x5B00)
            {
                ScreenBuffer.UpdateScreen(address);
            }
        };
        Cpu.Clock.TicksAdded += (_, previousFrameTicks, _) => ScreenBuffer.UpdateScreen(previousFrameTicks);
        Cpu.BeforeInstruction += BeforeInstruction;
        UlaPlus.ActiveChanged += (_) => _invalidateScreen = true;
    }

    private void SetupUlaAndDevices()
    {
        var ula = new Ula(KeyboardState, ScreenBuffer, Cpu.Clock, TapeManager?.CassettePlayer);

        _spectrumBus.AddDevice(ula);
        _spectrumBus.AddDevice(UlaPlus);
        _spectrumBus.AddDevice(_memory);
        _spectrumBus.AddDevice(AudioManager.Beeper);
        _spectrumBus.AddDevice(AudioManager.Ay);
        _spectrumBus.AddDevice(Printer);
        _spectrumBus.AddDevice(DivMmc);
        _spectrumBus.AddDevice(new RtcDevice(DivMmc));

        _floatingBus = new FloatingBus(_hardware, Memory, Cpu.Clock);
        _spectrumBus.AddDevice(_floatingBus);

        Cpu.AddBus(_spectrumBus);
    }

    private void RunFrame()
    {
        StartFrame();

        if (_isNmiRequested)
        {
            _isNmiRequested = false;

            Cpu.TriggerNmi();
        }

        Cpu.Run();

        EndFrame();

        if (_invalidateScreen)
        {
            ScreenBuffer.Invalidate();
            _invalidateScreen = false;
        }
    }

    private void StartFrame()
    {
        if (_isDebuggerResume)
        {
            _isDebuggerResume = false;

            return;
        }

        Cpu.Clock.NewFrame(_hardware.TicksPerFrame);
        ScreenBuffer.NewFrame();
        AudioManager.NewFrame();
    }

    private void EndFrame()
    {
        var audioBuffer = AudioManager.EndFrame();

        ScreenBuffer.EndFrame(Cpu.Clock.FrameTicks);

        FrameCompleted?.Invoke(ScreenBuffer.FrameBuffer, audioBuffer);

        _timeMachine.AddEntry(this);
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
                switch (TapeLoadSpeed)
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
            //    break;
        }
    }
}
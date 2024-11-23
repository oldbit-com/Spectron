using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.GamePad;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Devices.Memory;
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
    private readonly GamePadManager _gamePadManager;
    private readonly TimeMachine _timeMachine;
    private readonly SpectrumBus _spectrumBus;
    private readonly EmulatorTimer _emulationTimer;

    private bool _invalidateScreen;
    private bool _isAcceleratedTapeSpeed;

    public delegate void RenderScreenEvent(FrameBuffer frameBuffer);
    public event RenderScreenEvent? RenderScreen;

    public bool IsPaused => _emulationTimer.IsPaused;
    public bool IsUlaPlusEnabled { get => UlaPlus.IsEnabled; set => ToggleUlaPlus(value); }
    public KeyboardHandler KeyboardHandler { get; } = new();
    public TapeManager TapeManager { get; }
    public JoystickManager JoystickManager { get; }
    public AudioManager AudioManager { get; }
    public ComputerType ComputerType { get; }
    public RomType RomType { get; }
    public TapeSpeed TapeLoadSpeed { get; set; }

    internal Z80 Cpu { get; }
    internal IEmulatorMemory Memory { get; }
    internal ScreenBuffer ScreenBuffer { get; }
    internal UlaPlus UlaPlus { get; }

    internal Emulator(
        EmulatorArgs emulatorArgs,
        HardwareSettings hardware,
        TapeManager tapeManager,
        GamePadManager gamePadManager,
        TimeMachine timeMachine,
        ILogger logger)
    {
        TapeManager = tapeManager;
        _hardware = hardware;
        _gamePadManager = gamePadManager;
        _timeMachine = timeMachine;
        ComputerType = emulatorArgs.ComputerType;
        RomType = emulatorArgs.RomType;
        Memory = emulatorArgs.Memory;

        UlaPlus = new UlaPlus();
        _spectrumBus = new SpectrumBus();
        ScreenBuffer = new ScreenBuffer(hardware, emulatorArgs.Memory, UlaPlus);
        Cpu = new Z80(emulatorArgs.Memory, emulatorArgs.ContentionProvider); ;

        JoystickManager = new JoystickManager(gamePadManager, _spectrumBus, KeyboardHandler);
        TapeManager.Attach(Cpu, Memory, hardware);

        AudioManager = new AudioManager(Cpu.Clock, tapeManager.Player, hardware);

        SetupUlaAndDevices();
        SetupEventHandlers();

        _emulationTimer = new EmulatorTimer();
        _emulationTimer.Elapsed += _ =>
        {
            try
            {
                RunFrame();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during frame execution");
            }
        };
    }

    public void Start()
    {
        AudioManager.Start();
        _emulationTimer.Start();
    }

    public void Shutdown()
    {
        AudioManager.Stop();
        _emulationTimer.Stop();
        _gamePadManager.Stop();

        while (!_emulationTimer.IsStopped)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(50));
        }
    }

    public void Pause()
    {
        _emulationTimer.Pause();
        _timeMachine.AddEntry(this);
    }

    public void Resume() => _emulationTimer.Resume();

    public void Reset()
    {
        AudioManager.ResetAudio();
        Memory.Reset();
        Cpu.Reset();
        ScreenBuffer.Reset();
        UlaPlus.Reset();

        if (IsPaused)
        {
            Resume();
        }
    }

    public void SetEmulationSpeed(int emulationSpeedPercentage) => _emulationTimer.Interval =
        emulationSpeedPercentage == int.MaxValue ?
            TimeSpan.Zero :
            TimeSpan.FromMilliseconds(20 * (100f / emulationSpeedPercentage));

    private void SetupEventHandlers()
    {
        Memory.ScreenMemoryUpdated += address => ScreenBuffer.UpdateScreen(address);
        Cpu.Clock.TicksAdded += (_, _, currentFrameTicks) => ScreenBuffer.UpdateContent(currentFrameTicks);
        Cpu.BeforeFetch += BeforeInstructionFetch;
        UlaPlus.ActiveChanged += (_) => _invalidateScreen = true;
    }

    private void SetupUlaAndDevices()
    {
        var ula = new Ula(KeyboardHandler, ScreenBuffer, Cpu.Clock, TapeManager?.Player);

        _spectrumBus.AddDevice(ula);
        _spectrumBus.AddDevice(UlaPlus);
        _spectrumBus.AddDevice(Memory);
        _spectrumBus.AddDevice(AudioManager.Beeper);
        _spectrumBus.AddDevice(AudioManager.Ay);

        var floatingBus = new FloatingBus(_hardware, Memory, Cpu.Clock);
        _spectrumBus.AddDevice(floatingBus);

        Cpu.AddBus(_spectrumBus);
    }

    private void RunFrame()
    {
        StartFrame();

        Cpu.Run(_hardware.TicksPerFrame);

        EndFrame();

        if (_invalidateScreen)
        {
            ScreenBuffer.Invalidate();
            _invalidateScreen = false;
        }
    }

    private void StartFrame()
    {
        Cpu.Clock.NewFrame();
        ScreenBuffer.NewFrame();
        AudioManager.NewFrame();
    }

    private void EndFrame()
    {
        AudioManager.EndFrame();

        ScreenBuffer.UpdateBorder(Cpu.Clock.FrameTicks);
        Cpu.TriggerInt(0xFF);
        RenderScreen?.Invoke(ScreenBuffer.FrameBuffer);

        _timeMachine.AddEntry(this);
    }

    private void ToggleUlaPlus(bool value)
    {
        UlaPlus.IsEnabled = value;
        _invalidateScreen = true;
    }

    private void BeforeInstructionFetch(Word pc)
    {
        switch (pc)
        {
            case RomRoutines.LD_BYTES:
                switch (TapeLoadSpeed)
                {
                    case TapeSpeed.Instant:
                        TapeManager.LoadDirect();
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
                        TapeManager.SaveDirect();
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

            case 0x1AF1:
            case RomRoutines.SAVE_ETC:
                // var szx = SzxSnapshot.CreateSnapshot(this);
                // szx.Save("/Users/voytas/Projects/ZX/Spectron/src/Spectron.Emulation/Tape/Loader/Files/128.szx");

                break;
        }
    }
}
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation;

/// <summary>
/// This is the main emulator class that ties everything together.
/// </summary>
public sealed class Emulator
{
    private readonly TimeMachine _timeMachine;
    private readonly Beeper _beeper;
    private readonly SpectrumBus _spectrumBus;
    private readonly EmulatorTimer _emulationTimer;
    private bool _invalidateScreen;
    private bool _isAcceleratedLoading;

    public delegate void RenderScreenEvent(FrameBuffer frameBuffer);
    public event RenderScreenEvent? RenderScreen;

    public bool IsPaused => _emulationTimer.IsPaused;
    public bool IsUlaPlusEnabled { get => UlaPlus.IsEnabled; set => ToggleUlaPlus(value); }
    public KeyboardHandler KeyboardHandler { get; } = new();
    public TapeManager TapeManager { get; }
    public JoystickManager JoystickManager { get; }
    public ComputerType ComputerType { get; }
    public RomType RomType { get; }
    public TapeLoadingSpeed TapeLoadingSpeed { get; set; }

    internal Z80 Cpu { get; }
    internal IEmulatorMemory Memory { get; }
    internal ScreenBuffer ScreenBuffer { get; }
    internal UlaPlus UlaPlus { get; }

    internal Emulator(EmulatorSettings emulator, HardwareSettings hardware, TimeMachine timeMachine)
    {
        _timeMachine = timeMachine;
        ComputerType = emulator.ComputerType;
        RomType = emulator.RomType;
        Memory = emulator.Memory;
        _beeper = emulator.Beeper;

        UlaPlus = new UlaPlus();
        _spectrumBus = new SpectrumBus();
        ScreenBuffer = new ScreenBuffer(emulator.Memory, UlaPlus);
        Cpu = new Z80(emulator.Memory, emulator.ContentionProvider);

        TapeManager = new TapeManager(this, hardware);
        JoystickManager = new JoystickManager(_spectrumBus, KeyboardHandler);

        SetupUlaAndDevices(emulator.UseAYSound);
        SetupEventHandlers();

        _emulationTimer = new EmulatorTimer(RunFrame);
    }

    public void Start() => _emulationTimer.Start();

    public void Stop()
    {
        _beeper.Stop();
        _emulationTimer.Stop();
    }

    public void Pause()
    {
        _emulationTimer.Pause();
        _timeMachine.AddEntry(this);
    }

    public void Resume() => _emulationTimer.Resume();

    public void Reset()
    {
        Memory.Reset();
        Cpu.Reset();
        ScreenBuffer.Reset();
        UlaPlus.Reset();

        if (IsPaused)
        {
            Resume();
        }
    }

    public void SetEmulationSpeed(int emulationSpeedPercentage) =>
        _emulationTimer.Interval = TimeSpan.FromMilliseconds(20 * (100f / emulationSpeedPercentage));

    private void SetupEventHandlers()
    {
        Memory.ScreenMemoryUpdated += address => ScreenBuffer.UpdateScreen(address);
        Cpu.Clock.TicksAdded += (_, _, currentFrameTicks) => ScreenBuffer.UpdateContent(currentFrameTicks);
        Cpu.BeforeFetch += BeforeInstructionFetch;
        UlaPlus.ActiveChanged += (_) => _invalidateScreen = true;
    }

    private void SetupUlaAndDevices(bool useAYSound)
    {
        var ula = new Ula(KeyboardHandler, _beeper, ScreenBuffer, Cpu.Clock, TapeManager.TapePlayer);

        _spectrumBus.AddDevice(ula);
        _spectrumBus.AddDevice(UlaPlus);
        _spectrumBus.AddDevice(Memory);

        if (useAYSound)
        {
            _spectrumBus.AddDevice(new AY8910());
        }

        var floatingBus = new FloatingBus(Memory, Cpu.Clock);
        _spectrumBus.AddDevice(floatingBus);

        Cpu.AddBus(_spectrumBus);
    }

    private void RunFrame()
    {
        StartFrame();

        Cpu.Run(DefaultTimings.FrameTicks);

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
    }

    private void EndFrame()
    {
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
                if (TapeLoadingSpeed == TapeLoadingSpeed.Instant)
                {
                    TapeManager.InstantLoad();
                }
                else if (TapeLoadingSpeed == TapeLoadingSpeed.Accelerated)
                {
                    SetEmulationSpeed(1000);
                    _isAcceleratedLoading = true;
                }
                break;

            case RomRoutines.LD_BYTES_RET:
            case RomRoutines.ERROR_1:
                if (_isAcceleratedLoading)
                {
                    SetEmulationSpeed(100);
                    _isAcceleratedLoading = false;
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
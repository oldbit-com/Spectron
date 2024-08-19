using OldBit.Spectral.Emulation.Devices;
using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Devices.Joystick;
using OldBit.Spectral.Emulation.Devices.Keyboard;
using OldBit.Spectral.Emulation.Devices.Memory;
using OldBit.Spectral.Emulation.Rom;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Spectral.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulation;

/// <summary>
/// This is the main emulator class that ties everything together.
/// </summary>
public sealed class Emulator
{
    private readonly Beeper _beeper;
    private readonly UlaPlus _ulaPlus;
    private readonly SpectrumBus _spectrumBus;
    private readonly EmulatorTimer _emulationTimer;
    private bool _invalidateScreen;
    private bool _isAcceleratedLoading;

    public delegate void RenderScreenEvent(FrameBuffer frameBuffer);
    public event RenderScreenEvent? RenderScreen;

    public bool IsPaused => _emulationTimer.IsPaused;
    public bool IsUlaPlusEnabled { set => ToggleUlaPlus(value); }
    public KeyboardHandler KeyboardHandler { get; } = new();
    public TapeManager TapeManager { get; }
    public JoystickManager JoystickManager { get; }
    public ComputerType ComputerType { get; }
    public RomType RomType { get; }
    public TapeLoadingSpeed TapeLoadingSpeed { get; set; }

    internal Z80 Cpu { get; }
    internal IEmulatorMemory Memory { get; }
    internal ScreenBuffer ScreenBuffer { get; }

    internal Emulator(EmulatorSettings emulator, HardwareSettings hardware)
    {
        ComputerType = emulator.ComputerType;
        RomType = emulator.RomType;
        Memory = emulator.Memory;
        _beeper = emulator.Beeper;

        _ulaPlus = new UlaPlus();
        _spectrumBus = new SpectrumBus();
        ScreenBuffer = new ScreenBuffer(emulator.Memory, _ulaPlus);
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

    public void Pause() => _emulationTimer.Pause();

    public void Resume() => _emulationTimer.Resume();

    public void Reset()
    {
        Memory.Reset();
        Cpu.Reset();
        ScreenBuffer.Reset();
        _ulaPlus.Reset();
    }

    public void SetEmulationSpeed(int emulationSpeedPercentage) =>
        _emulationTimer.Interval = TimeSpan.FromMilliseconds(20 * (100f / emulationSpeedPercentage));

    public void LoadTape(string fileName)
    {
        if (!TapeManager.TryLoadTape(fileName))
        {
            return;
        }

        // TODO: Simulate LOAD "" to start the loader
    }

    private void SetupEventHandlers()
    {
        Memory.ScreenMemoryUpdated += address => ScreenBuffer.UpdateScreen(address);
        Cpu.Clock.TicksAdded += (_, _, currentFrameTicks) => ScreenBuffer.UpdateContent(currentFrameTicks);
        Cpu.BeforeFetch += BeforeInstructionFetch;
        _ulaPlus.ActiveChanged += (_) => _invalidateScreen = true;
    }

    private void SetupUlaAndDevices(bool useAYSound)
    {
        var ula = new Ula(KeyboardHandler, _beeper, ScreenBuffer, Cpu.Clock, TapeManager.TapePlayer);

        _spectrumBus.AddDevice(ula);
        _spectrumBus.AddDevice(_ulaPlus);
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
    }

    private void ToggleUlaPlus(bool value)
    {
        _ulaPlus.IsEnabled = value;
        _invalidateScreen = true;
    }

    private void BeforeInstructionFetch(Word pc)
    {
        switch (pc)
        {
            case RomRoutines.LD_BYTES:
                if (TapeLoadingSpeed == TapeLoadingSpeed.Instant)
                {
                    TapeManager.InstantTapeLoader.LoadBytes();
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

            case RomRoutines.SAVE_ETC:
                //
                break;
        }
    }
}
using System.Diagnostics;
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
    private readonly Thread _workerThread;
    private bool _isRunning;
    private bool _invalidateScreen;

    public delegate void RenderScreenEvent(FrameBuffer frameBuffer);
    public event RenderScreenEvent? RenderScreen;

    public bool IsPaused { get; private set; }
    public bool IsUlaPlusEnabled { set => ToggleUlaPlus(value); }
    public KeyboardHandler KeyboardHandler { get; } = new();
    public TapeManager TapeManager { get; }
    public JoystickManager JoystickManager { get; }

    internal Z80 Z80 { get; }
    internal IEmulatorMemory Memory { get; }
    internal ScreenBuffer ScreenBuffer { get; }

    internal Emulator(EmulatorSettings emulator, HardwareSettings hardware)
    {
        Memory = emulator.Memory;
        _beeper = emulator.Beeper;
        _ulaPlus = new UlaPlus();
        _spectrumBus = new SpectrumBus();
        ScreenBuffer = new ScreenBuffer(emulator.Memory, _ulaPlus);
        Z80 = new Z80(emulator.Memory, emulator.ContentionProvider);

        TapeManager = new TapeManager(this, hardware);
        JoystickManager = new JoystickManager(_spectrumBus, KeyboardHandler);

        SetupUlaAndDevices(emulator.UseAYSound);
        SetupEventHandlers();
        _workerThread = SetupWorkerThread();
    }

    public void Start()
    {
        _isRunning = true;
        _workerThread.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        _beeper.Stop();
        _workerThread.Join();
    }

    public void Pause() => IsPaused = true;

    public void Resume() => IsPaused = false;

    public void Reset()
    {
        Memory.Reset();
        Z80.Reset();
        ScreenBuffer.Reset();
    }

    private void SetupEventHandlers()
    {
        Memory.ScreenMemoryUpdated += address => ScreenBuffer.UpdateScreen(address);

        Z80.Clock.TicksAdded += (_, _, currentFrameTicks) => ScreenBuffer.UpdateContent(currentFrameTicks);

        Z80.BeforeFetch += BeforeInstructionFetch;

        _ulaPlus.ActiveChanged += (_) => _invalidateScreen = true;
    }

    private void SetupUlaAndDevices(bool useAYSound)
    {
        var ula = new Ula(Memory, KeyboardHandler, _beeper, ScreenBuffer, Z80.Clock, TapeManager.TapePlayer);

        _spectrumBus.AddDevice(ula);
        _spectrumBus.AddDevice(_ulaPlus);
        _spectrumBus.AddDevice(Memory);

        if (useAYSound)
        {
            _spectrumBus.AddDevice(new AY8910());
        }

        var floatingBus = new FloatingBus(Memory, Z80.Clock);
        _spectrumBus.AddDevice(floatingBus);

        Z80.AddBus(_spectrumBus);
    }

    private Thread SetupWorkerThread() => new(WorkerThread)
    {
        IsBackground = true,
        Priority = ThreadPriority.AboveNormal,
    };

    private void RunFrame()
    {
        StartFrame();

        Z80.Run(DefaultTimings.FrameTicks);

        EndFrame();

        if (_invalidateScreen)
        {
            ScreenBuffer.Invalidate();
            _invalidateScreen = false;
        }
    }

    private void StartFrame()
    {
        Z80.Clock.NewFrame();
        ScreenBuffer.NewFrame();
    }

    private void EndFrame()
    {
        ScreenBuffer.UpdateBorder(Z80.Clock.FrameTicks);

        Z80.TriggerInt(0xFF);

        RenderScreen?.Invoke(ScreenBuffer.FrameBuffer);
    }

    private void ToggleUlaPlus(bool value)
    {
        _ulaPlus.IsEnabled = value;
        _invalidateScreen = true;
    }

    private void WorkerThread()
    {
        var stopwatch = Stopwatch.StartNew();
        var interval = TimeSpan.FromMilliseconds(20);
        var nextTrigger = TimeSpan.Zero;
        var lastElapsed = 0L;

        while (_isRunning)
        {
            nextTrigger += interval;

            if (IsPaused)
            {
                Thread.Sleep(500);

                nextTrigger = TimeSpan.Zero;
                stopwatch.Restart();

                continue;
            }

            while (_isRunning)
            {
                var timeToWait = (nextTrigger - stopwatch.Elapsed).TotalMilliseconds;

                if (timeToWait <= 0)
                {
                    RunFrame();
                    // Console.WriteLine("Elapsed: " + (stopwatch.ElapsedMilliseconds - lastElapsed));
                    // lastElapsed = stopwatch.ElapsedMilliseconds;

                    break;
                }

                switch (timeToWait)
                {
                    case < 1:
                        Thread.SpinWait(5);
                        break;

                    case < 5:
                        Thread.SpinWait(10);
                        break;

                    case < 10:
                        Thread.Sleep(5);
                        break;
                }
            }
        }

        stopwatch.Stop();
    }

    private void BeforeInstructionFetch(Word pc)
    {
        switch (pc)
        {
            case 0x056A:
                TapeManager.FastFileLoader.LoadBytes();
                break;

            case RomRoutines.SAVE_ETC:
                //
                break;
        }
    }
}
using System.Diagnostics;
using OldBit.Spectral.Emulation.Devices;
using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Devices.Keyboard;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Spectral.Emulation.Tape;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Contention;

namespace OldBit.Spectral.Emulation.Computers;

/// <summary>
/// This is the main emulator class that ties everything together.
/// </summary>
public sealed class Emulator
{
    private readonly Beeper _beeper;
    private readonly Z80 _z80;
    private readonly ScreenRenderer _screenRenderer;
    private readonly Thread _workerThread;
    private bool _isRunning;

    public delegate void RenderScreenEvent(FrameBuffer frameBuffer);
    public event RenderScreenEvent? RenderScreen;

    public bool IsPaused { get; private set; }
    public KeyHandler KeyHandler { get; } = new();
    public TapeLoader TapeLoader { get; }

    public Emulator(
        Memory memory,
        Beeper beeper,
        IContentionProvider contentionProvider)
    {
        _beeper = beeper;
        _screenRenderer = new ScreenRenderer(memory);
        memory.ScreenMemoryUpdated += address => _screenRenderer.ScreenMemoryUpdated(address);

        _z80 = new Z80(memory, contentionProvider);
        _z80.Clock.TicksAdded += (_, _, currentFrameTicks) => _screenRenderer.UpdateContent(currentFrameTicks);

        var tapePlayer = new TapePlayer(_z80.Clock);

        var ula = new Ula(memory, KeyHandler, beeper, _screenRenderer, _z80.Clock, tapePlayer);
        var bus = new Bus();

        bus.AddInputDevice(ula);
        bus.AddOutputDevice(ula);
        _z80.AddBus(bus);

        TapeLoader = new TapeLoader(_z80, memory, _screenRenderer, tapePlayer);

        _workerThread = new Thread(WorkerThread)
        {
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal,
        };
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
        _z80.Reset();
        _screenRenderer.Reset();
    }

    private void RunFrame()
    {
        StartFrame();

        _z80.Run(DefaultTimings.FrameTicks);

        EndFrame();
    }

    private void StartFrame()
    {
        _z80.Clock.NewFrame();
        _screenRenderer.NewFrame();
    }

    private void EndFrame()
    {
        _screenRenderer.UpdateBorder(_z80.Clock.FrameTicks);

        _z80.INT(0xFF);

        RenderScreen?.Invoke(_screenRenderer.FrameBuffer);
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
}
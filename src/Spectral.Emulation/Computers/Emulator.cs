using System.Diagnostics;
using OldBit.Spectral.Emulation.Devices;
using OldBit.Spectral.Emulation.Devices.Audio;
using OldBit.Spectral.Emulation.Devices.Keyboard;
using OldBit.Spectral.Emulation.Devices.Memory;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Spectral.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulation.Computers;

/// <summary>
/// This is the main emulator class that ties everything together.
/// </summary>
public sealed class Emulator
{
    private readonly IEmulatorMemory _memory;
    private readonly Beeper _beeper;
    private readonly Z80 _z80;
    private readonly ScreenBuffer _screenBuffer;
    private readonly Thread _workerThread;
    private bool _isRunning;

    public delegate void RenderScreenEvent(FrameBuffer frameBuffer);
    public event RenderScreenEvent? RenderScreen;

    public bool IsPaused { get; private set; }
    public bool IsUlaPlusEnabled { get; set; }
    public KeyHandler KeyHandler { get; } = new();
    public TapeManager TapeManager { get; }

    internal Emulator(EmulatorSettings emulator, HardwareSettings hardware)
    {
        _memory = emulator.Memory;
        _beeper = emulator.Beeper;
        _screenBuffer = new ScreenBuffer(emulator.Memory);
        emulator.Memory.ScreenMemoryUpdated += address => _screenBuffer.UpdateScreen(address);

        _z80 = new Z80(emulator.Memory, emulator.ContentionProvider);
        _z80.Clock.TicksAdded += (_, _, currentFrameTicks) => _screenBuffer.UpdateContent(currentFrameTicks);

        TapeManager = new TapeManager(_z80, emulator.Memory, _screenBuffer, hardware);

        var ula = new Ula(emulator.Memory, KeyHandler, emulator.Beeper, _screenBuffer, _z80.Clock, TapeManager.TapePlayer);
        var bus = new Bus();

        bus.AddDevice(ula);
        bus.AddDevice(emulator.Memory);

        _z80.AddBus(bus);

        if (emulator.UseAYSound)
        {
            bus.AddDevice(new AY8910());
        }

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
        _memory.Reset();
        _z80.Reset();
        _screenBuffer.Reset();
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
        _screenBuffer.NewFrame();
    }

    private void EndFrame()
    {
        _screenBuffer.UpdateBorder(_z80.Clock.FrameTicks);

        _z80.INT(0xFF);

        RenderScreen?.Invoke(_screenBuffer.FrameBuffer);
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
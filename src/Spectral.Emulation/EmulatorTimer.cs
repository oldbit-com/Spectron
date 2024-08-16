using System.Diagnostics;

namespace OldBit.Spectral.Emulation;

/// <summary>
/// Custom timer that supports more accurate timing than the built-in .NET timer.
/// Standard timers are not accurate enough for emulators.
/// </summary>
internal sealed class EmulatorTimer
{
    private bool _isRunning;
    private readonly Action _callback;
    private readonly Thread _thread;

    internal bool IsPaused { get; private set; }

    public EmulatorTimer(Action callback)
    {
        _callback = callback;

        _thread = new Thread(Worker)
        {
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal
        };
    }

    internal void Start()
    {
        _thread.Start();
        _isRunning = true;
    }

    internal void Stop()
    {
        _isRunning = false;
        _thread.Join();
    }

    internal void Pause()
    {
        IsPaused = true;
    }

    internal void Resume()
    {
        IsPaused = false;
    }

    private void Worker()
    {
        var stopwatch = Stopwatch.StartNew();
        var interval = TimeSpan.FromMilliseconds(20);
        var nextTrigger = TimeSpan.Zero;

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
                    _callback();
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
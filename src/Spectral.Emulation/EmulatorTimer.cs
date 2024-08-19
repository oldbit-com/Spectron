using System.Diagnostics;

namespace OldBit.Spectral.Emulation;

/// <summary>
/// Custom timer that supports more accurate timing than the built-in .NET timer.
/// Standard timers are not accurate enough for emulators.
/// </summary>
internal sealed class EmulatorTimer
{
    private readonly Action _callback;
    private readonly Thread _thread;
    private bool _isRunning;

    internal bool IsPaused { get; private set; }
    internal TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(20);

    internal EmulatorTimer(Action callback)
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
        var nextTrigger = TimeSpan.Zero;

        var resetTimer = () =>
        {
            nextTrigger = TimeSpan.Zero;
            stopwatch.Restart();
        };

        while (_isRunning)
        {
            nextTrigger += Interval;

            if (IsPaused)
            {
                Thread.Sleep(500);

                resetTimer();

                continue;
            }

            while (_isRunning)
            {
                var timeToWait = (nextTrigger - stopwatch.Elapsed).TotalMilliseconds;

                if (timeToWait <= 0)
                {
                    if (timeToWait < -Interval.TotalMilliseconds)
                    {
                        resetTimer();
                    }

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
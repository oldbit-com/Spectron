using System.Diagnostics;

namespace OldBit.Spectron.Emulation;

/// <summary>
/// Custom timer that supports more accurate timing than the built-in .NET timer.
/// Standard timers are not accurate enough for emulators.
/// </summary>
internal sealed class EmulatorTimer
{
    private readonly Thread _thread;
    private bool _isRunning;

    internal bool IsPaused { get; private set; }
    internal TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(20);

    internal delegate void ElapsedEvent(EventArgs e);
    internal event ElapsedEvent? Elapsed;

    internal EmulatorTimer()
    {
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
        var timeToWait = Interval;

        while (_isRunning)
        {
            if (IsPaused)
            {
                Thread.Sleep(500);
                ResetTimer();

                continue;
            }

            while (_isRunning)
            {
                var elapsed = stopwatch.Elapsed;

                if (elapsed >= nextTrigger)
                {
                    if (timeToWait < -Interval)
                    {
                        ResetTimer();
                    }

                    Elapsed?.Invoke(EventArgs.Empty);
                    nextTrigger += Interval;

                    break;
                }

                timeToWait = nextTrigger - stopwatch.Elapsed;

                switch (timeToWait.TotalMilliseconds)
                {
                    case < 1:
                        Thread.SpinWait(1);
                        break;

                    case < 5:
                        Thread.SpinWait(2);
                        break;

                    case < 10:
                        Thread.SpinWait(5);
                        break;
                }
            }
        }

        stopwatch.Stop();
        return;

        void ResetTimer()
        {
            nextTrigger = TimeSpan.Zero;
            stopwatch.Restart();
        }
    }
}
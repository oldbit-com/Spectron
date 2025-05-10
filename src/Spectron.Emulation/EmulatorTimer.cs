using System.Diagnostics;

namespace OldBit.Spectron.Emulation;

/// <summary>
/// Custom timer that supports more accurate timing than the built-in .NET timer.
/// Standard timer does not have enough accuracy for the emulator. Stopwatch
/// is most accurate and used to calculate the time between each tick.
/// </summary>
internal sealed class EmulatorTimer
{
    private readonly Thread _worker;
    private bool _isRunning;

    internal bool IsPaused { get; private set; }
    internal bool IsStopped { get; private set; }

    internal TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(20);

    internal delegate void ElapsedEvent(EventArgs e);
    internal event ElapsedEvent? Elapsed;

    internal EmulatorTimer()
    {
        _worker = new Thread(Worker)
        {
            IsBackground = true,
            Priority = ThreadPriority.Lowest,
        };
    }

    internal void Start()
    {
        _isRunning = true;
        _worker.Start();
    }

    internal void Stop()
    {
        _isRunning = false;
        _worker.Join();
    }

    internal void Pause() => IsPaused = true;

    internal void Resume() => IsPaused = false;

    private void Worker()
    {
        IsStopped = false;

        var timer = Stopwatch.StartNew();
        var nextTrigger = TimeSpan.Zero;

        while (_isRunning)
        {
            if (IsPaused)
            {
                Thread.Sleep(250);
                nextTrigger = Interval;
                timer.Restart();

                continue;
            }

            while (_isRunning)
            {
                var elapsed = timer.Elapsed;

                if (elapsed >= nextTrigger)
                {
                    timer.Restart();
                    nextTrigger = Interval;

                    Elapsed?.Invoke(EventArgs.Empty);

                    break;
                }

                var timeToWait = nextTrigger - elapsed;

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

        IsStopped = true;
        timer.Stop();
    }
}
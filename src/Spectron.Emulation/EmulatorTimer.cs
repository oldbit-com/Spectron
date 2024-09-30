using System.Diagnostics;
using OldBit.Spectron.Emulation.Utilities;

namespace OldBit.Spectron.Emulation;

/// <summary>
/// Custom timer that supports more accurate timing than the built-in .NET timer.
/// </summary>
internal sealed class EmulatorTimer
{
    private readonly Thread _thread;
    private bool _isRunning;

    internal bool IsPaused { get; private set; }
    internal bool IsStopped { get; private set; }

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

    internal void Pause() => IsPaused = true;

    internal void Resume() => IsPaused = false;

    private void Worker()
    {
        IsStopped = false;

        var stopwatch = Stopwatch.StartNew();
        var nextTrigger = TimeSpan.Zero;

        while (_isRunning)
        {
            if (IsPaused)
            {
                Thread.Sleep(500);
                nextTrigger = Interval;
                stopwatch.Restart();

                continue;
            }

            while (_isRunning)
            {
                var elapsed = stopwatch.Elapsed;

                if (elapsed >= nextTrigger)
                {
                    stopwatch.Restart();
                    nextTrigger = Interval;

                   // PerformanceLogger.Instance.Log("TimerWorker");

                    Elapsed?.Invoke(EventArgs.Empty);

                    break;
                }

                var timeToWait = nextTrigger - stopwatch.Elapsed;

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
        stopwatch.Stop();
    }
}
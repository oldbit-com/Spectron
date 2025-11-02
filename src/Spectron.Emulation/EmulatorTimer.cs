using System.Diagnostics;

namespace OldBit.Spectron.Emulation;

/// <summary>
/// Custom timer that supports more accurate timing than the built-in .NET timer.
/// Standard timer does not have enough accuracy for the emulator.
/// </summary>
internal sealed class EmulatorTimer : IDisposable
{
    private readonly Thread _worker;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    internal bool IsPaused { get; private set; }
    internal bool IsStopped { get; private set; }
    internal ThreadPriority Priority { get; set; } = ThreadPriority.AboveNormal;

    internal TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(20);

    internal event EventHandler? Elapsed;

    internal EmulatorTimer() => _worker = new Thread(Worker)
    {
        IsBackground = true,
        Priority = Priority,
        Name = "Emulator Timer"
    };

    internal void Start() => _worker.Start();

    internal void Stop()
    {
        _cancellationTokenSource.Cancel();

        _worker.Join();
    }

    internal void Pause() => IsPaused = true;

    internal void Resume() => IsPaused = false;

    private void Worker()
    {
        IsStopped = false;

        var stopwatch = Stopwatch.StartNew();
        var nextTrigger = TimeSpan.Zero;

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (IsPaused)
            {
                Thread.Sleep(250);

                stopwatch.Restart();
                nextTrigger = Interval;

                continue;
            }

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var elapsed = stopwatch.Elapsed;

                if (elapsed >= nextTrigger)
                {
                    stopwatch.Restart();
                    nextTrigger = Interval;

                    Elapsed?.Invoke(this, EventArgs.Empty);
                    break;
                }

                var timeToWait = nextTrigger - elapsed;

                switch (timeToWait.TotalMilliseconds)
                {
                    case < 1:
                        Thread.SpinWait(5);
                        break;

                    case < 5:
                        Thread.SpinWait(10);
                        break;

                    case < 10:
                        Thread.SpinWait(25);
                        break;
                }
            }
        }

        IsStopped = true;
        stopwatch.Stop();
    }

    public void Dispose() => _cancellationTokenSource.Dispose();
}
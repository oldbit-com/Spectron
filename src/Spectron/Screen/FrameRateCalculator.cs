using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace OldBit.Spectron.Screen;

public sealed class FrameRateCalculator : IDisposable
{
    private readonly Timer _frameRateTimer;
    private int _framesPerSecond;

    public Action<int> FrameRateChanged { get; set; } = _ => { };

    public FrameRateCalculator()
    {
        _frameRateTimer = new Timer(TimeSpan.FromSeconds(1));
        _frameRateTimer.AutoReset = true;
        _frameRateTimer.Elapsed += FrameRateTimerElapsed;
    }

    private void FrameRateTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        FrameRateChanged(_framesPerSecond);
        Interlocked.Exchange(ref _framesPerSecond, 0);
    }

    public void Start() => _frameRateTimer.Start();

    public void FrameCompleted() => Interlocked.Increment(ref _framesPerSecond);

    public void Dispose() => _frameRateTimer.Dispose();
}
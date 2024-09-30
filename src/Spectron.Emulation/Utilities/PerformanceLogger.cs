using System.Diagnostics;

namespace OldBit.Spectron.Emulation.Utilities;

public sealed class PerformanceLogger
{
    public static PerformanceLogger Instance { get; } = new();

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private TimeSpan _lastElapsed = TimeSpan.Zero;


    public void Log(string message)
    {
        var elapsed = _stopwatch.Elapsed;
        var delta = elapsed - _lastElapsed;
        _lastElapsed = elapsed;

        Console.WriteLine($"{message}: {elapsed.TotalMilliseconds,10:0.000}ms (+{delta.TotalMilliseconds,4:0.000}ms)");
    }

    public void Reset()
    {
        _stopwatch.Restart();
        _lastElapsed = TimeSpan.Zero;
    }
}
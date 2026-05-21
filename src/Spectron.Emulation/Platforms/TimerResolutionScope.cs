using OldBit.Spectron.Emulation.Platforms.Windows.Interop;

namespace OldBit.Spectron.Emulation.Platforms;

internal sealed class TimerResolutionScope : IDisposable
{
    private const int TimerResolutionMs = 1;

    internal TimerResolutionScope()
    {
        if (OperatingSystem.IsWindows())
        {
            _ = Winmm.TimeBeginPeriod(TimerResolutionMs);
        }
    }

    public void Dispose()
    {
        if (OperatingSystem.IsWindows())
        {
            _ = Winmm.TimeEndPeriod(TimerResolutionMs);
        }
    }
}
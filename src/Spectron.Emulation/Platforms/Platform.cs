using OldBit.Spectron.Emulation.Platforms.Windows.Interop;

namespace OldBit.Spectron.Emulation.Platforms;

internal static class Platform
{
    internal static void RequestMinimumTimerResolution()
    {
        if (OperatingSystem.IsWindows())
        {
            _ = Winmm.TimeBeginPeriod(1);
        }
    }

    internal static void ReleaseMinimumTimerResolution()
    {
        if (OperatingSystem.IsWindows())
        {
            _ = Winmm.TimeEndPeriod(1);
        }
    }
}
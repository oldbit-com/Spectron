using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OldBit.Spectron.Emulation.Platforms.Windows.Interop;

[SupportedOSPlatform("windows")]
internal static partial class Winmm
{
    /// <summary>
    /// The timeBeginPeriod function requests a minimum resolution for periodic timers.
    /// </summary>
    /// <param name="uPeriod">Minimum timer resolution, in milliseconds, for the application or device driver.
    /// A lower value specifies a higher (more accurate) resolution.</param>
    /// <returns>Returns TIMERR_NOERROR if successful or TIMERR_NOCANDO if the resolution specified in uPeriod is out of range.</returns>
    [LibraryImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    internal static partial uint TimeBeginPeriod(uint uPeriod);

    /// <summary>
    /// The timeEndPeriod function clears a previously set minimum timer resolution.
    /// </summary>
    /// <param name="uPeriod">Minimum timer resolution specified in the previous call to the timeBeginPeriod function.</param>
    /// <returns>Returns TIMERR_NOERROR if successful or TIMERR_NOCANDO if the resolution specified in uPeriod is out of range.</returns>
    [LibraryImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    internal static partial uint TimeEndPeriod(uint uPeriod);
}
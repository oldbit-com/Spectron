using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OldBit.Spectron.Emulation.Platforms.Windows.Interop;

[SupportedOSPlatform("windows")]
internal static partial class Winmm
{
    [LibraryImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    internal static partial uint TimeBeginPeriod(uint uPeriod);

    [LibraryImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    internal static partial uint TimeEndPeriod(uint uPeriod);
}
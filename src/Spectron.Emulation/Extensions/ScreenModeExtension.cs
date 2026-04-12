using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulation.Extensions;

internal static class ScreenModeExtension
{
    internal static bool IsTimexHiRes(this ScreenMode screenMode) => ((int)screenMode & 0x04) == 0x04;
}
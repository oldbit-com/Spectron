namespace OldBit.ZXSpectrum.Emulator.Screen;

internal static class Constants
{
    internal const int BorderTop = 64;           // 64 lines of top border
    internal const int BorderLeft = 48;          // 48 pixels of left border
    internal const int BorderRight = 48;         // 48 pixels of right border
    internal const int BorderBottom = 56;        // 56 lines of bottom border

    internal const int ContentWidth = 256;       // 256 pixels of content
    internal const int ContentHeight = 192;      // 192 lines of content

    internal const int TotalLines = BorderTop + ContentHeight + BorderBottom;

    internal const int RetraceTicks = 48;
    internal const int ContentTicks = 128;
    internal const int LeftBorderTicks = 24;
    internal const int RightBorderTicks = 24;
    internal const int LineTicks = LeftBorderTicks + ContentTicks + RightBorderTicks + RetraceTicks;
    internal const int FrameTicks = TotalLines * LineTicks;
    internal const int FirstDataPixelTick = 14336;
}
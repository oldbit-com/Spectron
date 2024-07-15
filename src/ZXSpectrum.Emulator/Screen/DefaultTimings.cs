namespace OldBit.ZXSpectrum.Emulator.Screen;

internal static class DefaultTimings
{
    /// <summary>
    /// Number of ticks for the horizontal retrace (48T).
    /// </summary>
    internal const int RetraceTicks = 48;

    /// <summary>
    /// Number of ticks for the screen line content (128T).
    /// </summary>
    internal const int ContentLineTicks = 128;

    /// <summary>
    /// Number of ticks for the left border (24T).
    /// </summary>
    internal const int LeftBorderTicks = 24;

    /// <summary>
    /// Number of ticks for the right border (24T).
    /// </summary>
    internal const int RightBorderTicks = 24;

    /// <summary>
    /// Number of ticks for the whole screen line, e.g. including borders, content and retrace (224T).
    /// </summary>
    internal const int LineTicks = LeftBorderTicks + ContentLineTicks + RightBorderTicks + RetraceTicks;

    /// <summary>
    /// Number of ticks per frame (69888T.
    /// </summary>
    internal const int FrameTicks = DefaultSizes.TotalLines * LineTicks;

    internal const int FirstDataPixelTick = 14336;
}
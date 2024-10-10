namespace OldBit.Spectron.Emulation.Screen;

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
    /// Number of T-states passed since the interrupt generation to the first display byte is being sent to screen (early timing).
    /// </summary>
    internal const int FirstPixelTick = 14335;

    /// <summary>
    /// Number of T-states passed since the interrupt generation to the last display byte is being sent to screen.
    /// </summary>
    internal const int LastPixelTick = FirstPixelTick + (ScreenSize.ContentHeight - 1) * LineTicks + ContentLineTicks;
}
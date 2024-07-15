namespace OldBit.ZXSpectrum.Emulator.Screen;

internal static class DefaultSizes
{
    /// <summary>
    /// Standard top border size is 64 lines.
    /// </summary>
    internal const int BorderTop = 64;

    /// <summary>
    /// Standard left border size is 48 pixels.
    /// </summary>
    internal const int BorderLeft = 48;

    /// <summary>
    /// Standard right border size is 48 pixels.
    /// </summary>
    internal const int BorderRight = 48;

    /// <summary>
    /// Standard bottom border size is 56 lines.
    /// </summary>
    internal const int BorderBottom = 56;

    /// <summary>
    /// The width of the screen content is 256 pixels, e.g. 32 columns of 8 pixels.
    /// </summary>
    internal const int ContentWidth = 256;

    /// <summary>
    /// The height of the screen content is 192 pixels, e.g. 24 rows of 8 pixels.
    /// </summary>
    internal const int ContentHeight = 192;

    internal const int TotalLines = BorderTop + ContentHeight + BorderBottom;
}
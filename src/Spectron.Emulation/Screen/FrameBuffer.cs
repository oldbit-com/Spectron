namespace OldBit.Spectron.Emulation.Screen;

/// <summary>
/// Represents a frame buffer for the ZX Spectrum screen.
/// </summary>
public sealed class FrameBuffer(Color fillColor)
{
    public static int Width => ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight;

    public static int Height => ScreenSize.BorderTop + ScreenSize.ContentHeight + ScreenSize.BorderBottom;

    public readonly Color[] Pixels = Enumerable.Repeat(fillColor, Width * Height).ToArray();

    internal void Fill(int start, int count, Color color) => Array.Fill(Pixels, color, start, count);

    internal static int GetLineIndex(int line, int borderTop) => Width * borderTop + ScreenSize.BorderLeft + Width * line;
}
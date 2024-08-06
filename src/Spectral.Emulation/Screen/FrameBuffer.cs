namespace OldBit.Spectral.Emulation.Screen;

/// <summary>
/// Represents a frame buffer for the ZX Spectrum screen.
/// </summary>
public class FrameBuffer(Color color)
{
    public static int Width => ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight;

    public static int Height => ScreenSize.BorderTop + ScreenSize.ContentHeight + ScreenSize.BorderBottom;

    public Color[] Pixels { get; } = Enumerable.Repeat(color, Width * Height).ToArray();

    internal void Fill(int start, int count, Color color)
    {
        Array.Fill(Pixels, color, start, count);
    }

    internal static int GetLineIndex(int line) => Width * ScreenSize.BorderTop + ScreenSize.BorderLeft + Width * line;
}
namespace OldBit.ZXSpectrum.Emulator.Screen;

/// <summary>
/// Represents a frame buffer for the ZX Spectrum screen.
/// </summary>
public class FrameBuffer
{
    public static int Width => DefaultSizes.BorderLeft + DefaultSizes.ContentWidth + DefaultSizes.BorderRight;

    public static int Height => DefaultSizes.BorderTop + DefaultSizes.ContentHeight + DefaultSizes.BorderBottom;

    public Color[] Pixels { get; }

    public FrameBuffer(Color color)
    {
        Pixels = Enumerable.Repeat(color, Width * Height).ToArray();
    }

    internal void Fill(int start, int count, Color color)
    {
        Array.Fill(Pixels, color, start, count);
    }

    internal static int GetLineIndex(int line) => Width * DefaultSizes.BorderTop + DefaultSizes.BorderLeft + Width * line;
}
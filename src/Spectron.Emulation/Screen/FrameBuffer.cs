namespace OldBit.Spectron.Emulation.Screen;

/// <summary>
/// Represents a buffer for the ZX Spectrum screen with predefined dimensions
/// that includes all pixels for both content and border areas.
/// </summary>
public sealed class FrameBuffer
{
    private static readonly Color DefaultFillColor = SpectrumPalette.White;

    internal ScreenMode ScreenMode { get; }
    public int Width { get; }
    public int Height { get; }

    public readonly Color[] Pixels;

    public FrameBuffer(ScreenMode screenMode = ScreenMode.Spectrum)
    {
        ScreenMode = screenMode;

        var hiResMultiplier = screenMode == ScreenMode.TimexHiRes ? 2 : 1;

        Width = ScreenSize.BorderLeft + ScreenSize.ContentWidth * hiResMultiplier + ScreenSize.BorderRight;
        Height = ScreenSize.BorderTop + ScreenSize.ContentHeight + ScreenSize.BorderBottom;

        Pixels = Enumerable.Repeat(DefaultFillColor, Width * Height).ToArray();
    }

    internal void Fill(int start, int count, Color color) => Array.Fill(Pixels, color, start, count);

    internal int GetLineIndex(int line, int borderTop) => Width * borderTop + ScreenSize.BorderLeft + Width * line;
}
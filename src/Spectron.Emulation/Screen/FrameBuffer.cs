namespace OldBit.Spectron.Emulation.Screen;

/// <summary>
/// Represents a buffer for the ZX Spectrum screen with predefined dimensions
/// that includes all pixels for both content and border areas.
/// </summary>
public sealed class FrameBuffer
{
    private static readonly Color DefaultFillColor = SpectrumPalette.White;

    internal ScreenMode ScreenMode { get; private set; } = ScreenMode.Spectrum;
    public int Width { get; private set; } = ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight;
    public int Height { get; private set; } = ScreenSize.BorderTop + ScreenSize.ContentHeight + ScreenSize.BorderBottom;

    public readonly Color[] Pixels;

    public FrameBuffer()
    {
        const int maxWidth = ScreenSize.BorderLeft + ScreenSize.ContentWidth * 2 + ScreenSize.BorderRight;

        ChangeScreenMode(ScreenMode);

        Pixels = Enumerable.Repeat(DefaultFillColor, maxWidth * Height).ToArray();
    }

    internal void ChangeScreenMode(ScreenMode screenMode)
    {
        if (ScreenMode == screenMode)
        {
            return;
        }

        ScreenMode = screenMode;

        var hiResMultiplier = screenMode == ScreenMode.TimexHiRes ? 2 : 1;

        Width = ScreenSize.BorderLeft + ScreenSize.ContentWidth * hiResMultiplier + ScreenSize.BorderRight;
        Height = ScreenSize.BorderTop + ScreenSize.ContentHeight + ScreenSize.BorderBottom;
    }

    internal void Fill(int start, int count, Color color) => Array.Fill(Pixels, color, start, count);

    internal int GetLineIndex(int line, int borderTop) => Width * borderTop + ScreenSize.BorderLeft + Width * line;
}
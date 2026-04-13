using OldBit.Spectron.Emulation.Extensions;

namespace OldBit.Spectron.Emulation.Screen;

/// <summary>
/// Represents a buffer for the ZX Spectrum screen with predefined dimensions
/// that includes all pixels for both content and border areas.
/// </summary>
public sealed class FrameBuffer
{
    private static readonly Color DefaultFillColor = SpectrumPalette.White;
    private int _borderLeft = ScreenSize.BorderLeft;

    internal ScreenMode ScreenMode { get; private set; } = ScreenMode.Spectrum;

    public int Width { get; private set; } = ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight;
    public int Height { get; private set; } = ScreenSize.BorderTop + ScreenSize.ContentHeight + ScreenSize.BorderBottom;
    public bool IsHiRes => ScreenMode.IsTimexHiRes();

    public readonly Color[] Pixels;

    public FrameBuffer()
    {
        const int hiResWidth = (ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight) * 2;

        ChangeScreenMode(ScreenMode);

        Pixels = Enumerable.Repeat(DefaultFillColor, hiResWidth * Height).ToArray();
    }

    internal void ChangeScreenMode(ScreenMode screenMode)
    {
        if (ScreenMode == screenMode)
        {
            return;
        }

        ScreenMode = screenMode;

        var hiResMultiplier = IsHiRes ? 2 : 1;
        _borderLeft = ScreenSize.BorderLeft * hiResMultiplier;

        Width = (ScreenSize.BorderLeft + ScreenSize.ContentWidth + ScreenSize.BorderRight) * hiResMultiplier;
        Height = ScreenSize.BorderTop + ScreenSize.ContentHeight + ScreenSize.BorderBottom;
    }

    internal void Fill(int start, int count, Color color) => Array.Fill(Pixels, color, start, count);

    internal int GetLineIndex(int line, int borderTop) => Width * borderTop + _borderLeft + Width * line;
}
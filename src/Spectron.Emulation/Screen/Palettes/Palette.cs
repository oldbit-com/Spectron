namespace OldBit.Spectron.Emulation.Screen.Palettes;

public record Bright(PaletteColor Ink, PaletteColor Paper);

public sealed class Palette
{
    public PaletteColor Ink { get; } = new(
        SpectrumPalette.Black,
        SpectrumPalette.Blue,
        SpectrumPalette.Red,
        SpectrumPalette.Magenta,
        SpectrumPalette.Green,
        SpectrumPalette.Cyan,
        SpectrumPalette.Yellow,
        SpectrumPalette.White);

    public PaletteColor Paper { get; } = new(
        SpectrumPalette.Black,
        SpectrumPalette.Blue,
        SpectrumPalette.Red,
        SpectrumPalette.Magenta,
        SpectrumPalette.Green,
        SpectrumPalette.Cyan,
        SpectrumPalette.Yellow,
        SpectrumPalette.White);

    public Bright Bright { get; } = new(
        new PaletteColor(
            SpectrumPalette.Black,
            SpectrumPalette.BrightBlue,
            SpectrumPalette.BrightRed,
            SpectrumPalette.BrightMagenta,
            SpectrumPalette.BrightGreen,
            SpectrumPalette.BrightCyan,
            SpectrumPalette.BrightYellow,
            SpectrumPalette.BrightWhite),

        new PaletteColor(
            SpectrumPalette.Black,
            SpectrumPalette.BrightBlue,
            SpectrumPalette.BrightRed,
            SpectrumPalette.BrightMagenta,
            SpectrumPalette.BrightGreen,
            SpectrumPalette.BrightCyan,
            SpectrumPalette.BrightYellow,
            SpectrumPalette.BrightWhite));

    public PaletteColor Border { get; } = new(
        SpectrumPalette.Black,
        SpectrumPalette.Blue,
        SpectrumPalette.Red,
        SpectrumPalette.Magenta,
        SpectrumPalette.Green,
        SpectrumPalette.Cyan,
        SpectrumPalette.Yellow,
        SpectrumPalette.White);
}
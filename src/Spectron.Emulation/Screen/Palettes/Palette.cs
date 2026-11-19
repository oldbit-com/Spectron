namespace OldBit.Spectron.Emulation.Screen.Palettes;

public record Bright(PaletteColor Ink, PaletteColor Paper);

public sealed class Palette
{
    public PaletteColor Ink { get; init; } = SpectrumPalette.StandardColors;

    public PaletteColor Paper { get; init; } = SpectrumPalette.StandardColors;

    public Bright Bright { get; init; } = new(SpectrumPalette.StandardBrightColors, SpectrumPalette.StandardBrightColors);

    public PaletteColor Border { get; init; } = SpectrumPalette.StandardColors;
}

namespace OldBit.Spectron.Emulation.Screen.Palettes;

public sealed record PaletteColor
(
    Color Black,
    Color Blue,
    Color Red,
    Color Magenta,
    Color Green,
    Color Cyan,
    Color Yellow,
    Color White
)
{
    public const int Count = 8;

    public Color[] ToArray() => [Black, Blue, Red, Magenta, Green, Cyan, Yellow, White];

    public static PaletteColor From(ReadOnlySpan<Color> colors)
    {
        return colors.Length != Count ?
            throw new ArgumentException($"Expected {Count} colors but got {colors.Length}.", nameof(colors)) :
            new PaletteColor(colors[0], colors[1], colors[2], colors[3], colors[4], colors[5], colors[6], colors[7]);
    }
}

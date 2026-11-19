using System.Diagnostics.CodeAnalysis;

namespace OldBit.Spectron.Emulation.Screen.Palettes;

public static class SpectrumPalette
{
    internal static readonly Color Black = new (0x00, 0x00, 0x00);
    internal static readonly Color Blue = new (0x00, 0x00, 0xD7);
    internal static readonly Color Red = new (0xD7, 0x00, 0x00);
    internal static readonly Color Magenta = new (0xD7, 0x00, 0xD7);
    internal static readonly Color Green = new (0x00, 0xD7, 0x00);
    internal static readonly Color Cyan = new (0x00, 0xD7, 0xD7);
    internal static readonly Color Yellow = new (0xD7, 0xD7, 0x00);
    internal static readonly Color White = new (0xD7, 0xD7, 0xD7);

    internal static readonly Color BrightBlue = new (0x00, 0x00, 0xFF);
    internal static readonly Color BrightRed = new (0xFF, 0x00, 0x00);
    internal static readonly Color BrightMagenta = new (0xFF, 0x00, 0xFF);
    internal static readonly Color BrightGreen = new (0x00, 0xFF, 0x00);
    internal static readonly Color BrightCyan = new (0x00, 0xFF, 0xFF);
    internal static readonly Color BrightYellow = new (0xFF, 0xFF, 0x00);
    internal static readonly Color BrightWhite = new (0xFF, 0xFF, 0xFF);

    internal static readonly PaletteColor StandardColors =
        new(Black, Blue, Red, Magenta, Green, Cyan, Yellow, White);

    internal static readonly PaletteColor StandardBrightColors =
        new(Black, BrightBlue, BrightRed, BrightMagenta, BrightGreen, BrightCyan, BrightYellow, BrightWhite);

    private static readonly Color[] BorderColors = new Color[0x08];
    private static readonly Color[] InkColors = new Color[0x48];
    private static readonly Color[] PaperColors = new Color[0x80];

    public static Palette Current { get; private set; }

    public static Color GetBorderColor(int attr) => BorderColors[attr & 0x07];
    public static Color GetPaperColor(int attr) => PaperColors[attr & 0x78];
    public static Color GetInkColor(int attr) => InkColors[attr & 0x47];

    static SpectrumPalette() => SetPalette(new Palette());

    [MemberNotNull(nameof(Current))]
    internal static void SetPalette(Palette palette)
    {
        Current = palette;

        var border = palette.Border.ToArray();
        var ink = palette.Ink.ToArray();
        var inkBright = palette.Bright.Ink.ToArray();
        var paper = palette.Paper.ToArray();
        var paperBright = palette.Bright.Paper.ToArray();

        for (var i = 0; i < PaletteColor.Count; i++)
        {
            BorderColors[i] = border[i];

            InkColors[i] = ink[i];                              // bits 0-2: ink
            InkColors[0x40 | i] = inkBright[i];                 // bit 6: bright

            PaperColors[i << 3] = paper[i];                     // bits 3-5: paper
            PaperColors[0x40 | (i << 3)] = paperBright[i];      // bit 6: bright
        }
    }
}

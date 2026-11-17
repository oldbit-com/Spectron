using System.Diagnostics.CodeAnalysis;

namespace OldBit.Spectron.Emulation.Screen.Palettes;

public static class SpectrumPalette
{
    public static Palette Current { get; private set; }

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

    public static Color GetBorderColor(int attr) => BorderColors[(byte)(attr & 0x07)];
    public static Color GetPaperColor(int attr) => PaperColors[(byte)(attr & 0x78)];
    public static Color GetInkColor(int attr) => InkColors[(byte)(attr & 0x47)];

    static SpectrumPalette() => SetPalette(new Palette());

    [MemberNotNull(nameof(Current))]
    public static void SetPalette(Palette palette)
    {
        Current = palette;

        BorderColors[0b0000000] = palette.Border.Black;
        BorderColors[0b0000001] = palette.Border.Blue;
        BorderColors[0b0000010] = palette.Border.Red;
        BorderColors[0b0000011] = palette.Border.Magenta;
        BorderColors[0b0000100] = palette.Border.Green;
        BorderColors[0b0000101] = palette.Border.Cyan;
        BorderColors[0b0000110] = palette.Border.Yellow;
        BorderColors[0b0000111] = palette.Border.White;

        InkColors[0b00000000] = palette.Ink.Black;
        InkColors[0b00000001] = palette.Ink.Blue;
        InkColors[0b00000010] = palette.Ink.Red;
        InkColors[0b00000011] = palette.Ink.Magenta;
        InkColors[0b00000100] = palette.Ink.Green;
        InkColors[0b00000101] = palette.Ink.Cyan;
        InkColors[0b00000110] = palette.Ink.Yellow;
        InkColors[0b00000111] = palette.Ink.White;
        InkColors[0b01000000] = palette.Bright.Ink.Black;
        InkColors[0b01000001] = palette.Bright.Ink.Blue;
        InkColors[0b01000010] = palette.Bright.Ink.Red;
        InkColors[0b01000011] = palette.Bright.Ink.Magenta;
        InkColors[0b01000100] = palette.Bright.Ink.Green;
        InkColors[0b01000101] = palette.Bright.Ink.Cyan;
        InkColors[0b01000110] = palette.Bright.Ink.Yellow;
        InkColors[0b01000111] = palette.Bright.Ink.White;

        PaperColors[0b00000000] = palette.Paper.Black;
        PaperColors[0b00001000] = palette.Paper.Blue;
        PaperColors[0b00010000] = palette.Paper.Red;
        PaperColors[0b00011000] = palette.Paper.Magenta;
        PaperColors[0b00100000] = palette.Paper.Green;
        PaperColors[0b00101000] = palette.Paper.Cyan;
        PaperColors[0b00110000] = palette.Paper.Yellow;
        PaperColors[0b00111000] = palette.Paper.White;
        PaperColors[0b01000000] = palette.Bright.Paper.Black;
        PaperColors[0b01001000] = palette.Bright.Paper.Blue;
        PaperColors[0b01010000] = palette.Bright.Paper.Red;
        PaperColors[0b01011000] = palette.Bright.Paper.Magenta;
        PaperColors[0b01100000] = palette.Bright.Paper.Green;
        PaperColors[0b01101000] = palette.Bright.Paper.Cyan;
        PaperColors[0b01110000] = palette.Bright.Paper.Yellow;
        PaperColors[0b01111000] = palette.Bright.Paper.White;

        FastLookup.UpdateAttributeData();
    }

    private static readonly Dictionary<byte, Color> BorderColors = new();

    private static readonly Dictionary<int, Color> PaperColors = new();

    private static readonly Dictionary<int, Color> InkColors = new();
}
namespace OldBit.Spectron.Emulation.Screen;

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

    public static Color GetBorderColor(int attr) => BorderColors[(byte)(attr & 0x07)];
    public static Color GetPaperColor(int attr) => PaperColors[(byte)(attr & 0x78)];
    public static Color GetInkColor(int attr) => InkColors[(byte)(attr & 0x47)];

    private static readonly Dictionary<byte, Color> BorderColors = new()
    {
        { 0b0000000, Black },
        { 0b0000001, Blue },
        { 0b0000010, Red },
        { 0b0000011, Magenta },
        { 0b0000100, Green },
        { 0b0000101, Cyan },
        { 0b0000110, Yellow },
        { 0b0000111, White }
    };

    internal static readonly Dictionary<Color, byte> ReverseBorderColors =
        BorderColors.ToDictionary(x => x.Value, x => x.Key);

    private static readonly Dictionary<int, Color> PaperColors = new()
    {
        { 0b00000000, Black },
        { 0b00001000, Blue },
        { 0b00010000, Red },
        { 0b00011000, Magenta },
        { 0b00100000, Green },
        { 0b00101000, Cyan },
        { 0b00110000, Yellow },
        { 0b00111000, White },
        { 0b01000000, Black },
        { 0b01001000, BrightBlue },
        { 0b01010000, BrightRed },
        { 0b01011000, BrightMagenta },
        { 0b01100000, BrightGreen },
        { 0b01101000, BrightCyan },
        { 0b01110000, BrightYellow },
        { 0b01111000, BrightWhite }
    };

    private static readonly Dictionary<int, Color> InkColors = new()
    {
        { 0b00000000, Black },
        { 0b00000001, Blue },
        { 0b00000010, Red },
        { 0b00000011, Magenta },
        { 0b00000100, Green },
        { 0b00000101, Cyan },
        { 0b00000110, Yellow },
        { 0b00000111, White },
        { 0b01000000, Black },
        { 0b01000001, BrightBlue },
        { 0b01000010, BrightRed },
        { 0b01000011, BrightMagenta },
        { 0b01000100, BrightGreen },
        { 0b01000101, BrightCyan },
        { 0b01000110, BrightYellow },
        { 0b01000111, BrightWhite }
    };
}
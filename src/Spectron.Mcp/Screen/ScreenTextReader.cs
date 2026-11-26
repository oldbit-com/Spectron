using System.Text;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Mcp.Screen;

/// <summary>
/// Converts the Spectrum display file into text by matching every 8x8 character cell
/// against a character set. Much cheaper and more reliable for an agent than reading a screenshot.
/// </summary>
internal sealed class ScreenTextReader
{
    internal const int Rows = 24;
    internal const int Columns = 32;

    internal const char UnknownChar = '▒';

    private const int RomFontAddress = 0x3D00;
    private const int FirstChar = 0x20;
    private const int CharCount = 96;

    // Spectrum block graphics 0x80-0x8F: bit 0 = top right, bit 1 = top left, bit 2 = bottom right, bit 3 = bottom left.
    internal const string BlockGraphics = " ▝▘▀▗▐▚▜▖▞▌▛▄▟▙█";

    private static readonly Lazy<byte[]> RomFont = new(() =>
        RomReader.ReadRom(RomType.Original48).AsSpan(RomFontAddress, CharCount * 8).ToArray());

    private readonly Dictionary<ulong, char> _patterns = new();

    internal ScreenTextReader(ReadOnlySpan<byte> font)
    {
        AddBlockGraphics();
        AddFont(font);
    }

    /// <summary>
    /// Creates a reader using the standard ROM character set, or a custom one located in memory.
    /// </summary>
    internal static ScreenTextReader Create(IEmulatorMemory memory, Word? fontAddress = null)
    {
        if (fontAddress == null)
        {
            return new ScreenTextReader(RomFont.Value);
        }

        var font = new byte[CharCount * 8];

        for (var i = 0; i < font.Length; i++)
        {
            font[i] = memory.Read((Word)(fontAddress.Value + i));
        }

        return new ScreenTextReader(font);
    }

    /// <summary>
    /// Reads the 24 rows of the screen. Blank cells become spaces, cells that do not match
    /// any known character become <see cref="UnknownChar"/>. Inverse characters are recognised too.
    /// </summary>
    internal string[] ReadLines(IEmulatorMemory memory)
    {
        var lines = new string[Rows];
        var line = new StringBuilder(Columns);
        Span<byte> cell = stackalloc byte[8];

        for (var row = 0; row < Rows; row++)
        {
            line.Clear();

            for (var column = 0; column < Columns; column++)
            {
                for (var pixelLine = 0; pixelLine < 8; pixelLine++)
                {
                    cell[pixelLine] = memory.ReadScreen(GetBitmapAddress(row, pixelLine, column));
                }

                line.Append(Recognize(cell));
            }

            lines[row] = line.ToString().TrimEnd();
        }

        return lines;
    }

    internal char Recognize(ReadOnlySpan<byte> cell)
    {
        var pattern = ToPattern(cell);

        if (_patterns.TryGetValue(pattern, out var c) || _patterns.TryGetValue(~pattern, out c))
        {
            return c;
        }

        return UnknownChar;
    }

    internal static Word GetBitmapAddress(int row, int pixelLine, int column) =>
        (Word)(((row & 0x18) << 8) | (pixelLine << 8) | ((row & 0x07) << 5) | column);

    private void AddFont(ReadOnlySpan<byte> font)
    {
        for (var i = 0; i < CharCount; i++)
        {
            var pattern = ToPattern(font.Slice(i * 8, 8));
            var c = (char)(FirstChar + i) switch
            {
                '`' => '£',
                '\x7F' => '©',
                var ch => ch
            };

            _patterns.TryAdd(pattern, c);
        }
    }

    private void AddBlockGraphics()
    {
        for (var i = 0; i < BlockGraphics.Length; i++)
        {
            var top = (byte)(((i & 2) != 0 ? 0xF0 : 0) | ((i & 1) != 0 ? 0x0F : 0));
            var bottom = (byte)(((i & 8) != 0 ? 0xF0 : 0) | ((i & 4) != 0 ? 0x0F : 0));

            Span<byte> cell = [top, top, top, top, bottom, bottom, bottom, bottom];

            _patterns.TryAdd(ToPattern(cell), BlockGraphics[i]);
        }
    }

    private static ulong ToPattern(ReadOnlySpan<byte> cell)
    {
        var pattern = 0UL;

        foreach (var b in cell)
        {
            pattern = pattern << 8 | b;
        }

        return pattern;
    }
}

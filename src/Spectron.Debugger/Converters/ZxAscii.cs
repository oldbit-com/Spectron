using System.Text;

namespace OldBit.Spectron.Debugger.Converters;

/// <summary>
/// Converts ZX Spectrum ASCII codes to their Unicode equivalents.
/// </summary>
public static class ZxAscii
{
    public static char ToChar(byte value, char nonPrintChar = '.')
    {
        if (value >= 0x20 & value <= 0x8F)
        {
            return ToSpectrumChar(value);
        }

        return nonPrintChar;
    }

    public static string ToString(byte value, char nonPrintChar = '.') => ToChar(value, nonPrintChar).ToString();

    public static string ToString(ReadOnlySpan<byte> values, char nonPrintChar = '.')
    {
        var s = new StringBuilder(values.Length);

        foreach (var value in values)
        {
            s.Append(ToChar(value, nonPrintChar));
        }

        return s.ToString();
    }

    public static byte[] FromString(ReadOnlySpan<char> s)
    {
        var bytes = new byte[s.Length];

        for (var i = 0; i < s.Length; i++)
        {
            bytes[i] = FromSpectrumChar(s[i]);
        }

        return bytes;
    }

    private static char ToSpectrumChar(byte code) => code switch
    {
        0x5E => '\x2191', // "↑"
        0x60 => '\x00A3', // "£"
        0x7F => '\x00A9', // "©"
        0x80 => '\x0020', // " "
        0x81 => '\x259D', // "▝"
        0x82 => '\x2598', // "▘"
        0x83 => '\x2580', // "▀"
        0x84 => '\x2597', // "▗"
        0x85 => '\x2590', // "▐"
        0x86 => '\x259A', // "▚"
        0x87 => '\x259C', // "▜"
        0x88 => '\x2596', // "▖"
        0x89 => '\x259E', // "▞"
        0x8A => '\x258C', // "▌"
        0x8B => '\x259B', // "▛"
        0x8C => '\x2584', // "▄"
        0x8D => '\x259F', // "▟"
        0x8E => '\x2599', // "▙"
        0x8F => '\x2588', // "█"
        _ => Convert.ToChar(code)
    };

    public static byte FromSpectrumChar(char value) => value switch
    {
        '\u2191' => 0x5E, // "↑"
        '\u00A3' => 0x60, // "£"
        '\u00A9' => 0x7F, // "©"
        '\u259D' => 0x81, // "▝"
        '\u2598' => 0x82, // "▘"
        '\u2580' => 0x83, // "▀"
        '\u2597' => 0x84, // "▗"
        '\u2590' => 0x85, // "▐"
        '\u259A' => 0x86, // "▚"
        '\u259C' => 0x87, // "▜"
        '\u2596' => 0x88, // "▖"
        '\u259E' => 0x89, // "▞"
        '\u258C' => 0x8A, // "▌"
        '\u259B' => 0x8B, // "▛"
        '\u2584' => 0x8C, // "▄"
        '\u259F' => 0x8D, // "▟"
        '\u2599' => 0x8E, // "▙"
        '\u2588' => 0x8F, // "█"
        _ => (byte)value        // Only works for ASCII characters
    };
}
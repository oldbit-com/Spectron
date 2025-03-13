using System.Globalization;
using Avalonia.Data.Converters;

namespace OldBit.Spectron.Debugger.Converters;

public class ZxAsciiConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            byte code
                when code > 0x20 & code < 0x90 => ToSpectrumCharCode(code).ToString(),
            _ => "."
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static char ToSpectrumCharCode(byte code) =>
        code switch
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
            _ => System.Convert.ToChar(code)
        };
}
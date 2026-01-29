using System.Globalization;
using Avalonia.Data.Converters;
using OldBit.Spectron.Debugger.Controls.Hex;

namespace OldBit.Spectron.Debugger.Converters;

public class ZxAsciiConverter : IValueConverter, IAsciiFormatter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte code)
        {
            return ZxAscii.ToString(code);
        }

        return ".";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public char Format(byte b) => ZxAscii.ToChar(b);
}
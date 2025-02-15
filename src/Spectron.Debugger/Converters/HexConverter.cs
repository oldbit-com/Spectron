using System.Globalization;
using Avalonia.Data.Converters;
using OldBit.Spectron.Disassembly.Formatters;

namespace OldBit.Spectron.Debugger.Converters;

public class HexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        Word wordValue => NumberFormatter.Format(wordValue, NumberFormat.Hex),
        byte byteValue => NumberFormatter.Format(byteValue, NumberFormat.Hex),
        _ => value
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string address)
        {
            return Word.Parse(address.Replace("0x", string.Empty), NumberStyles.HexNumber);
        }

        return value;
    }
}
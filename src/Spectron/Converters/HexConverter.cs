using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OldBit.Spectron.Converters;

public class HexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        Word wordValue => $"{wordValue:X4}",
        byte byteValue => $"{byteValue:X2}",
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
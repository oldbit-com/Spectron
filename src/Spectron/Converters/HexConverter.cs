using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OldBit.Spectron.Converters;

public class HexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        ushort wordValue => $"0x{wordValue:X4}",
        byte byteValue => $"0x{byteValue:X2}",
        _ => value
    };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string address)
        {
            return ushort.Parse(address.Replace("0x", string.Empty), NumberStyles.HexNumber);
        }

        return value;
    }
}
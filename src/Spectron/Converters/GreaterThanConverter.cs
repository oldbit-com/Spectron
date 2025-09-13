using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OldBit.Spectron.Converters;

public class GreaterThanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int source && int.TryParse(parameter?.ToString(), out var comparison))
        {
            return source > comparison;
        }

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}
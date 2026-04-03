using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OldBit.Spectron.Converters;

public class EnumHasValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue || parameter is not Enum parameterValue)
        {
            return false;
        }

        return enumValue.HasFlag(parameterValue);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
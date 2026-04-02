using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OldBit.Spectron.Converters;

public class BoolToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b || parameter is not string text)
        {
            return string.Empty;
        }

        var items = text.Split('|');

        return b ? items[0] : items[1];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
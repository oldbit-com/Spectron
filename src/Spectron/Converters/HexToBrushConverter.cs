using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace OldBit.Spectron.Converters;

public class HexToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string text && Color.TryParse(text, out var color))
        {
            return new ImmutableSolidColorBrush(color);
        }

        return BindingOperations.DoNothing;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ISolidColorBrush brush)
        {
            return BindingOperations.DoNothing;
        }

        var color = brush.Color;

        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    }
}

using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace OldBit.Spectron.Converters;

public class HexColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ISolidColorBrush brush)
        {
            return $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}";
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = HexToColor(value as string);

        if (color is null)
        {
            return BindingOperations.DoNothing;
        }

        return new ImmutableSolidColorBrush(color.Value);
    }

    private static uint? HexToColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return null;
        }

        hex = hex.TrimStart('#');

        if (hex.Length != 6 || !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
        {
            return null;
        }

        return 0xFF000000u | rgb;
    }
}
using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Immutable;
using OldBit.Spectron.Extensions;

namespace OldBit.Spectron.Converters;

public class HexToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        ColorExtensions.TryParseHex(value as string, out var color) ?
            new ImmutableSolidColorBrush(color) :
            BindingOperations.DoNothing;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

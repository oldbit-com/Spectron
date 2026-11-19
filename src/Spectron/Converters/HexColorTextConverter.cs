using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using OldBit.Spectron.Extensions;

namespace OldBit.Spectron.Converters;

public class HexColorTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value as string ?? string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && ColorExtensions.TryParseHex(text, out _) ?
            text.Trim() :
            BindingOperations.DoNothing;
}

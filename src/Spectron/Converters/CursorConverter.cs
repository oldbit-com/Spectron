using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Input;
using OldBit.Spectron.Models;

namespace OldBit.Spectron.Converters;

public class CursorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value switch
        {
            MouseCursors.None => Cursor.Parse("None"),
            MouseCursors.Default => Cursor.Default,
            _ => throw new ArgumentException("Invalid cursor value provided")
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
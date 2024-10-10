using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace OldBit.Spectron.Converters;

public class IsFullScreenConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not WindowState windowState)
        {
            return false;
        }

        if (parameter is "invert")
        {
            return windowState != WindowState.FullScreen;
        }

        return windowState == WindowState.FullScreen;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
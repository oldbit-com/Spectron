using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using OldBit.Spectral.Models;

namespace OldBit.Spectral.Converters;

public class SelectedBorderSizeToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
        {
            return false;
        }

        if (value is BorderSize borderSize && parameter is BorderSize mathcingBorderSize)
        {
            return borderSize == mathcingBorderSize;
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using OldBit.Spectral.Emulator.Rom;

namespace OldBit.Spectral.Converters;

public class SelectedRomTypeToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
        {
            return false;
        }

        if (value is RomType romType && parameter is RomType matchingRomType)
        {
            return romType == matchingRomType;
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
}
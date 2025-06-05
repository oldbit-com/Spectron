using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OldBit.Spectron.Converters;

public class TapeProgressToWidthConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is not [double totalWidth, double progress])
        {
            return 0.0;
        }

        return totalWidth * Math.Clamp(progress, 0, 100) / 100.0;
    }
}
using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Converters;

public class ScreenshotBytesToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not byte[] bytes)
        {
            return null;
        }

        using var uncompressed = ScreenshotViewModel.Decompress(bytes);

        return new Bitmap(uncompressed);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
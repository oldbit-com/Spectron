using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Emulation.Devices.Printer;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class PrintOutputViewModel : ReactiveObject
{
    private readonly ZxPrinter _printer;
    private readonly int _height;

    public PrintOutputViewModel(ZxPrinter printer)
    {
        _printer = printer;

        _height = int.Max(printer.Rows.Count, 1024);
        const int width = 256;

        var bitmap = CreateBitmap(width, _height);

        OutputImage = bitmap;

        Update();
    }

    private void Update()
    {
        using var bitmap = OutputImage.Lock();

        unsafe
        {
            var buffer = (byte*)bitmap.Address;

            for (var row = 0; row < _printer.Rows.Count; row++)
            {
                for (var column = 0; column < _printer.Rows[row].Pixels.Length; column++)
                {
                    buffer[row * 32 + column] = (byte)~_printer.Rows[row].Pixels[column];
                }
            }

            for (var row = _printer.Rows.Count; row < _height; row++)
            {
                for (var column = 0; column < 32; column++)
                {
                    buffer[row * 32 + column] = 0xFF;
                }
            }
        }
    }

    private WriteableBitmap CreateBitmap(int width, int height)
    {
        return new WriteableBitmap(
            new PixelSize(
                width,
                height),
            new Vector(96, 96),
            PixelFormats.BlackWhite);
    }

    private WriteableBitmap _outputImage = null!;
    public WriteableBitmap OutputImage
    {
        get => _outputImage;
        set => this.RaiseAndSetIfChanged(ref _outputImage, value);
    }
}
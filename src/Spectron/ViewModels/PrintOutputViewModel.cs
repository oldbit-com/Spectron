using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Devices.Printer;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class PrintOutputViewModel : ReactiveObject
{
    private readonly ZxPrinter _printer;

    private int _height;

    public Control? PreviewControl { get; set; }

    public ReactiveCommand<Unit, Unit> ClearCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; }
    public ReactiveCommand<Unit, Task> SaveCommand { get; private set; }

    public PrintOutputViewModel(ZxPrinter printer)
    {
        ClearCommand = ReactiveCommand.Create(Clear);
        RefreshCommand = ReactiveCommand.Create(UpdatePreview);
        SaveCommand = ReactiveCommand.Create(Save);

        _printer = printer;

        UpdatePreview();
    }

    private void CrateBitmap()
    {
        _height = int.Max(_printer.Rows.Count, 512);

        OutputImage = CreateBitmap(256, _height);
    }

    private void UpdatePreview()
    {
        CrateBitmap();

        using var bitmap = OutputImage.Lock();

        unsafe
        {
            var buffer = (byte*)bitmap.Address;

            for (var row = 0; row < _printer.Rows.Count; row++)
            {
                for (var column = 0; column < _printer.Rows[row].Pixels.Length; column++)
                {
                    buffer[row * 32 + column] = (byte)~_printer.Rows[row].Pixels[column];;
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

        PreviewControl?.InvalidateVisual();
    }

    private void Clear()
    {
        _printer.Rows.Clear();
        UpdatePreview();
    }

    private async Task Save()
    {
        try
        {
            var file = await FileDialogs.SaveImageAsync("Save Printout", PreviewControl, "printout.png");

            if (file != null)
            {
                await using var output = File.OpenWrite(file.Path.LocalPath);

                OutputImage.Save(output, 100);
            }
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
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
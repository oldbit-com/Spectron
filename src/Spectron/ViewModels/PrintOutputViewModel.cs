using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Devices.Printer;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.ViewModels;

public partial class PrintOutputViewModel : ObservableObject
{
    private static readonly Color Black = new(0x00, 0x00, 0x00);
    private static readonly Color White = new(0xD8, 0xD8, 0xD8);
    private static readonly byte[] Masks = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];

    private readonly ZxPrinter _printer;
    private int _height;

    [ObservableProperty]
    private WriteableBitmap _outputImage = null!;

    public Control? PreviewControl { get; set; }

    public PrintOutputViewModel(ZxPrinter printer)
    {
        _printer = printer;
        UpdatePreview();
    }

    [RelayCommand]
    private void Clear()
    {
        _printer.Rows.Clear();
        UpdatePreview();
    }

    [RelayCommand]
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

    private void CrateBitmap()
    {
        _height = int.Max(_printer.Rows.Count, 512);

        OutputImage = CreateBitmap(256, _height);
    }

    [RelayCommand]
    private void UpdatePreview()
    {
        CrateBitmap();

        using var bitmap = OutputImage.Lock();

        var targetAddress = bitmap.Address;

        foreach (var row in _printer.Rows)
        {
            foreach (var cell in row.Pixels)
            {
                for (var bit = 0; bit < 8; bit++)
                {
                    var color = (cell & Masks[bit]) != 0 ? Black : White;

                    unsafe
                    {
                        *(uint*)targetAddress = *(uint*)&color;
                    }

                    targetAddress += 4;
                }
            }
        }

        PreviewControl?.InvalidateVisual();
    }

    private static WriteableBitmap CreateBitmap(int width, int height) => new(
        new PixelSize(width, height),
        new Vector(96, 96),
        PixelFormats.Rgba8888);
}
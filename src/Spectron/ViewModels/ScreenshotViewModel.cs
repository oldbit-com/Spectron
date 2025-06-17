using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Dialogs;

namespace OldBit.Spectron.ViewModels;

public record ScreenViewModel(byte[] Data);

public partial class ScreenshotViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public ObservableCollection<ScreenViewModel> Screenshots { get; } = [];

    [RelayCommand]
    private void Remove(ScreenViewModel item)
    {
        Screenshots.Remove(item);
    }

    [RelayCommand]
    private async Task Save(ScreenViewModel viewModel)
    {
        try
        {
            var file = await FileDialogs.SaveImageAsync("Save Screenshot", Window, "screen.png");

            if (file != null)
            {
                using var data = Decompress(viewModel.Data);
                await using var output = File.OpenWrite(file.Path.LocalPath);

                await data.CopyToAsync(output);
            }
        }
        catch (Exception ex)
        {
            await MessageDialogs.Error(ex.Message);
        }
    }

    public void AddScreenshot(Bitmap? screen)
    {
        if (screen is null)
        {
            return;
        }

        using var memoryStream = new MemoryStream();

        using (var compressed = new GZipStream(memoryStream, CompressionLevel.Fastest))
        {
            screen.Save(compressed, 100);
        }

        Screenshots.Add(new ScreenViewModel(memoryStream.ToArray()));
    }

    public static MemoryStream Decompress(byte[] data)
    {
        using var compressed = new MemoryStream(data);
        using var gzip = new GZipStream(compressed, CompressionMode.Decompress);
        var uncompressed = new MemoryStream();

        gzip.CopyTo(uncompressed);
        uncompressed.Seek(0, SeekOrigin.Begin);

        return uncompressed;
    }
}
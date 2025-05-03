using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using OldBit.Spectron.Dialogs;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public record ScreenViewModel(byte[] Data);

public class ScreenshotViewModel : ReactiveObject
{
    public Window? Window { get; set; }

    public ObservableCollection<ScreenViewModel> Screenshots { get; } = [];

    public ReactiveCommand<ScreenViewModel, Unit> RemoveCommand { get; private set; }
    public ReactiveCommand<ScreenViewModel, Task> SaveCommand { get; private set; }

    public ScreenshotViewModel()
    {
        RemoveCommand = ReactiveCommand.Create<ScreenViewModel>(item => Screenshots.Remove(item));
        SaveCommand = ReactiveCommand.Create<ScreenViewModel, Task>(SaveScreenshot);
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

    private async Task SaveScreenshot(ScreenViewModel viewModel)
    {
        try
        {
            var file = await FileDialogs.SaveImageAsync("Save Screenshot", Window,"screen.png");

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
}
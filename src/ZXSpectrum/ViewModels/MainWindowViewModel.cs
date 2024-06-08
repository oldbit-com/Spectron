using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using OldBit.ZXSpectrum.Emulator;
using OldBit.ZXSpectrum.Helpers;
using ReactiveUI;

namespace OldBit.ZXSpectrum.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public Window MainWindow { get; set; } = null!;

    public ISpectrum Spectrum { get; set; } = null!;

    public ReactiveCommand<Unit, Task> OpenFileCommand { get; private set; }

    public MainWindowViewModel()
    {
        OpenFileCommand = ReactiveCommand.Create(OpenFileAsync);
    }

    private async Task OpenFileAsync()
    {
        var topLevel = TopLevel.GetTopLevel(MainWindow);
        if (topLevel != null)
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    TapeFileTypes.All, TapeFileTypes.Sna, TapeFileTypes.Tap, TapeFileTypes.Tzx, TapeFileTypes.Z80
                }
            });

            if (files?.Count > 0)
            {
                try
                {
                    Spectrum.LoadFile(files[0].Name);
                }
                catch (Exception ex)
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, windowStartupLocation: WindowStartupLocation.CenterOwner);
                    await messageBox.ShowWindowDialogAsync(MainWindow);
                }
            }
        }
    }
}
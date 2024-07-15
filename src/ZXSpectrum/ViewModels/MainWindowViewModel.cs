using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using OldBit.ZXSpectrum.Emulator.Computers;
using OldBit.ZXSpectrum.Helpers;
using OldBit.ZXSpectrum.Models;
using ReactiveUI;

namespace OldBit.ZXSpectrum.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private BorderSize _borderSize = BorderSize.Full;

    public Window MainWindow { get; set; } = null!;

    public ISpectrum Spectrum { get; set; } = null!;

    public ReactiveCommand<Unit, Task> OpenFileCommand { get; private set; }

    public ReactiveCommand<BorderSize, Unit> ChangeBorderSizeCommand { get; private set; }

    public MainWindowViewModel()
    {
        OpenFileCommand = ReactiveCommand.Create(HandleOpenFileAsync);
        ChangeBorderSizeCommand = ReactiveCommand.Create<BorderSize>(HandleChangeBorderSize);
    }

    private async Task HandleOpenFileAsync()
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
                    Spectrum.Pause();
                    Spectrum.LoadFile(files[0].Path.LocalPath);
                }
                catch (Exception ex)
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message,
                        windowStartupLocation: WindowStartupLocation.CenterOwner);
                    await messageBox.ShowWindowDialogAsync(MainWindow);
                }
                finally
                {
                    Spectrum.Resume();
                }
            }
        }
    }

    private void HandleChangeBorderSize(BorderSize borderSize)
    {
        BorderSize = borderSize;
        // Handle border size change here
    }

    public BorderSize BorderSize
    {
        get => _borderSize;
        set => this.RaiseAndSetIfChanged(ref _borderSize, value);
    }
}
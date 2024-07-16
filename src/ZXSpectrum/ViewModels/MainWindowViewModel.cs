using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MsBox.Avalonia;
using OldBit.ZXSpectrum.Emulator.Computers;
using OldBit.ZXSpectrum.Emulator.Screen;
using OldBit.ZXSpectrum.Helpers;
using OldBit.ZXSpectrum.Models;
using ReactiveUI;

namespace OldBit.ZXSpectrum.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FrameBufferConverter _frameBufferConverter = new(4, 4);

    public Spectrum48K? Emulator { get; set; }

    public Window MainWindow { get; set; } = null!;

    public Control ScreenControl { get; set; } = null!;

    public MainWindowViewModel()
    {
        OpenFileCommand = ReactiveCommand.Create(HandleOpenFileAsync);
        ChangeBorderSizeCommand = ReactiveCommand.Create<BorderSize>(HandleChangeBorderSize);
    }

    public void Initialize()
    {
        Emulator = new Spectrum48K();
        Emulator.RenderScreen += EmulatorOnRenderScreen;

        Emulator.Start();
    }

    private void EmulatorOnRenderScreen(FrameBuffer framebuffer)
    {
        Dispatcher.UIThread.Post(() =>
        {
            SpectrumScreen = _frameBufferConverter.Convert(framebuffer);
            ScreenControl.InvalidateVisual();
        });
    }

    public ReactiveCommand<Unit, Task> OpenFileCommand { get; private set; }
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

            if (files.Count > 0)
            {
                try
                {
                    Emulator?.Pause();
                    Emulator?.LoadFile(files[0].Path.LocalPath);
                }
                catch (Exception ex)
                {
                    var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message,
                        windowStartupLocation: WindowStartupLocation.CenterOwner);
                    await messageBox.ShowWindowDialogAsync(MainWindow);
                }
                finally
                {
                    Emulator?.Resume();
                }
            }
        }
    }

    public ReactiveCommand<BorderSize, Unit> ChangeBorderSizeCommand { get; private set; }
    private void HandleChangeBorderSize(BorderSize borderSize)
    {
        BorderSize = borderSize;
        // Handle border size change here
    }

    private BorderSize _borderSize = BorderSize.Full;
    public BorderSize BorderSize
    {
        get => _borderSize;
        set => this.RaiseAndSetIfChanged(ref _borderSize, value);
    }

    private Bitmap? _spectrumScreen;
    public Bitmap? SpectrumScreen
    {
        get => _spectrumScreen;
        set => this.RaiseAndSetIfChanged(ref _spectrumScreen, value);
    }
}
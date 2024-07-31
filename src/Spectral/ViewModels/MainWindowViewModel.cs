using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MsBox.Avalonia;
using OldBit.Spectral.Emulator.Computers;
using OldBit.Spectral.Emulator.Screen;
using OldBit.Spectral.Helpers;
using OldBit.Spectral.Models;
using ReactiveUI;

namespace OldBit.Spectral.ViewModels;

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
        PauseCommand = ReactiveCommand.Create(HandleMachinePause, this.WhenAnyValue(x => x.Emulator).Select(emulator => emulator is null));
        ResetCommand = ReactiveCommand.Create(HandleMachineReset, this.WhenAnyValue(x => x.Emulator).Select(emulator => emulator is null));
    }

    public void Initialize()
    {
        Emulator = new Spectrum48K();
        Emulator.RenderScreen += EmulatorOnRenderScreen;

       // this.WhenAnyValue(x => x.Emulator!.IsPaused).ToProperty(this, x => x.IsPaused, out _isPaused);


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
                    TapeFileTypes.All, TapeFileTypes.Sna, TapeFileTypes.Szx, TapeFileTypes.Tap, TapeFileTypes.Tzx, TapeFileTypes.Z80
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
        _frameBufferConverter.SetBorderSize(borderSize);
    }

    public ReactiveCommand<Unit, Unit> ResetCommand { get; private set; }
    private void HandleMachineReset()
    {
        Emulator?.Reset();
    }

    public ReactiveCommand<Unit, Unit> PauseCommand { get; private set; }
    private void HandleMachinePause()
    {
        switch (Emulator?.IsPaused)
        {
            case true:
                Emulator.Resume();
                break;

            case false:
                Emulator.Pause();
                break;
        }

        IsPaused = Emulator?.IsPaused ?? false;
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

    private bool _isPaused;
    public bool IsPaused
    {
        get => _isPaused;
        set => this.RaiseAndSetIfChanged(ref _isPaused, value);
    }
}
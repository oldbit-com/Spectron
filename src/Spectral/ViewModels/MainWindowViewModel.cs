using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
using Timer = System.Timers.Timer;

namespace OldBit.Spectral.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FrameBufferConverter _frameBufferConverter = new(4, 4);
    private readonly Timer _statusBarTimer;
    private readonly Stopwatch _stopwatch = new();

    private int _frameCount;

    public Spectrum48K? Emulator { get; private set; }
    public Window MainWindow { get; set; } = null!;
    public Control ScreenControl { get; set; } = null!;

    public MainWindowViewModel()
    {
        _statusBarTimer = new Timer(TimeSpan.FromSeconds(1));
        _statusBarTimer.AutoReset = true;
        _statusBarTimer.Elapsed += StatusBarTimerOnElapsed;

        OpenFileCommand = ReactiveCommand.Create(HandleOpenFileAsync);
        ChangeBorderSizeCommand = ReactiveCommand.Create<BorderSize>(HandleChangeBorderSize);
        PauseCommand = ReactiveCommand.Create(HandleMachinePause, this.WhenAnyValue(x => x.Emulator).Select(emulator => emulator is null));
        ResetCommand = ReactiveCommand.Create(HandleMachineReset, this.WhenAnyValue(x => x.Emulator).Select(emulator => emulator is null));
    }

    private void StatusBarTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        var fps = $"FPS: {_frameCount.ToString()}";

        Dispatcher.UIThread.Post(() =>
        {
            StatusBar.FramesPerSecond = fps;
        });

        Interlocked.Exchange(ref _frameCount, 0);
    }

    public void Initialize()
    {
        Emulator = new Spectrum48K();
        Emulator.RenderScreen += EmulatorOnRenderScreen;
        SpectrumScreen = _frameBufferConverter.Bitmap;

        Emulator.Start();
        _statusBarTimer.Start();
    }

    private void EmulatorOnRenderScreen(FrameBuffer framebuffer)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _frameBufferConverter.UpdateBitmap(framebuffer);
            ScreenControl.InvalidateVisual();
        });

        Interlocked.Increment(ref _frameCount);
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
        SpectrumScreen = _frameBufferConverter.Bitmap;
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

    public StatusBarViewModel StatusBar { get; } = new();

    private BorderSize _borderSize = BorderSize.Full;
    public BorderSize BorderSize
    {
        get => _borderSize;
        set => this.RaiseAndSetIfChanged(ref _borderSize, value);
    }

    private WriteableBitmap? _spectrumScreen;
    public WriteableBitmap? SpectrumScreen
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
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using OldBit.Spectral.Dialogs;
using OldBit.Spectral.Emulation.Computers;
using OldBit.Spectral.Emulation.Devices.Keyboard;
using OldBit.Spectral.Emulation.Rom;
using OldBit.Spectral.Emulation.Screen;
using OldBit.Spectral.Helpers;
using OldBit.Spectral.Models;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace OldBit.Spectral.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FrameBufferConverter _frameBufferConverter = new(4, 4);
    private readonly Timer _statusBarTimer;

    private Emulator? Emulator { get; set; }
    private int _frameCount;

    public Control ScreenControl { get; set; } = null!;

    public TapeMenuViewModel TapeMenuViewModel { get; } = new();

    public ReactiveCommand<Unit, Task> OpenFileCommand { get; private set; }
    public ReactiveCommand<BorderSize, Unit> ChangeBorderSizeCommand { get; private set; }
    public ReactiveCommand<RomType, Unit> ChangeRomCommand { get; private set; }
    public ReactiveCommand<ComputerType, Unit> ChangeComputerType { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleUlaPlus { get; private set; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; private set; }

    public MainWindowViewModel()
    {
        _statusBarTimer = new Timer(TimeSpan.FromSeconds(1));
        _statusBarTimer.AutoReset = true;
        _statusBarTimer.Elapsed += StatusBarTimerOnElapsed;

        var emulatorNotNull = this.WhenAnyValue(x => x.Emulator).Select(emulator => emulator is null);

        OpenFileCommand = ReactiveCommand.Create(HandleOpenFileAsync);
        ChangeBorderSizeCommand = ReactiveCommand.Create<BorderSize>(HandleChangeBorderSize);
        ChangeRomCommand = ReactiveCommand.Create<RomType>(HandleChangeRom);
        ChangeComputerType = ReactiveCommand.Create<ComputerType>(HandleChangeComputerType);
        ToggleUlaPlus = ReactiveCommand.Create(HandleToggleUlaPlus);
        PauseCommand = ReactiveCommand.Create(HandleMachinePause, emulatorNotNull);
        ResetCommand = ReactiveCommand.Create(HandleMachineReset, emulatorNotNull);

        SpectrumScreen = _frameBufferConverter.Bitmap;
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

    public void Initialize(ComputerType computerType)
    {
        Emulator = EmulatorFactory.Create(computerType, RomType);
        Emulator.RenderScreen += EmulatorOnRenderScreen;
        TapeMenuViewModel.TapeManager = Emulator.TapeManager;

        Emulator.Start();
        _statusBarTimer.Start();
    }

    public void KeyDown(List<SpectrumKey> keys) => Emulator?.KeyHandler.HandleKeyDown(keys);

    public void KeyUp(List<SpectrumKey> keys) => Emulator?.KeyHandler.HandleKeyUp(keys);

    private void EmulatorOnRenderScreen(FrameBuffer framebuffer)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _frameBufferConverter.UpdateBitmap(framebuffer);
            ScreenControl.InvalidateVisual();
        });

        Interlocked.Increment(ref _frameCount);
    }

    private async Task HandleOpenFileAsync()
    {
        var files = await FileDialogs.OpenAnyFileAsync();

        if (files.Count > 0)
        {
            try
            {
                Emulator?.Pause();
                Emulator?.TapeManager.LoadAndRun(files[0].Path.LocalPath);
            }
            catch (Exception ex)
            {
                await MessageDialogs.Error(ex.Message);
            }
            finally
            {
                Emulator?.Resume();
            }
        }
    }

    private void HandleChangeBorderSize(BorderSize borderSize)
    {
        if (BorderSize == borderSize)
        {
            return;
        }

        BorderSize = borderSize;

        _frameBufferConverter.SetBorderSize(borderSize);
        SpectrumScreen = _frameBufferConverter.Bitmap;
    }

    private void HandleChangeRom(RomType romType)
    {
        if (RomType == romType)
        {
            return;
        }

        RomType = romType;

        if (Emulator != null)
        {
            Emulator.Stop();
            Emulator.RenderScreen -= EmulatorOnRenderScreen;
        }

        Initialize(ComputerType);
    }

    private void HandleChangeComputerType(ComputerType computerType)
    {
        if (ComputerType == computerType)
        {
            return;
        }

        ComputerType = computerType;

        Emulator?.Stop();
        Initialize(computerType);
    }

    private void HandleToggleUlaPlus()
    {
        IsUlaPlusEnabled = !IsUlaPlusEnabled;
        if (Emulator != null)
        {
            Emulator.IsUlaPlusEnabled = IsUlaPlusEnabled;
        }
    }

    private void HandleMachineReset()
    {
        Emulator?.Reset();
    }

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

    private RomType _romType = RomType.Original48;
    public RomType RomType
    {
        get => _romType;
        set => this.RaiseAndSetIfChanged(ref _romType, value);
    }

    private ComputerType _computerType = ComputerType.Spectrum48K;
    public ComputerType ComputerType
    {
        get => _computerType;
        set => this.RaiseAndSetIfChanged(ref _computerType, value);
    }

    private bool _isUlaPlusEnabled;
    public bool IsUlaPlusEnabled
    {
        get => _isUlaPlusEnabled;
        set => this.RaiseAndSetIfChanged(ref _isUlaPlusEnabled, value);
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
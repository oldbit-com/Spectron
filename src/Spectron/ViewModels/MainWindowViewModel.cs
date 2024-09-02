using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.File;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Helpers;
using OldBit.Spectron.Models;
using OldBit.Spectron.Preferences;
using OldBit.Spectron.Views;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace OldBit.Spectron.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FrameBufferConverter _frameBufferConverter = new(4, 4);
    private readonly Timer _statusBarTimer;

    private Emulator? Emulator { get; set; }
    private DefaultSettings _defaultSettings = new();
    private HelpKeyboardView? _helpKeyboardView;

    private int _frameCount;
    private readonly Stopwatch _renderStopwatch = new();
    private TimeSpan _lastScreenRender = TimeSpan.Zero;

    public Control ScreenControl { get; set; } = null!;
    public Window? MainWindow { get; set; }
    public StatusBarViewModel StatusBar { get; } = new();
    public TapeMenuViewModel TapeMenuViewModel { get; } = new();

    public ReactiveCommand<Unit, Unit> WindowOpenedCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> WindowClosingCommand { get; private set; }
    public ReactiveCommand<KeyEventArgs, Unit> KeyDownCommand { get; private set; }
    public ReactiveCommand<KeyEventArgs, Unit> KeyUpCommand { get; private set; }
    public ReactiveCommand<Unit, Task> LoadFileCommand { get; private set; }
    public ReactiveCommand<Unit, Task> SaveFileCommand { get; private set; }
    public ReactiveCommand<BorderSize, Unit> ChangeBorderSizeCommand { get; private set; }
    public ReactiveCommand<RomType, Unit> ChangeRomCommand { get; private set; }
    public ReactiveCommand<ComputerType, Unit> ChangeComputerType { get; private set; }
    public ReactiveCommand<JoystickType, Unit> ChangeJoystickType { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleUlaPlus { get; private set; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; private set; }
    public ReactiveCommand<string, Unit> SetEmulationSpeedCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleFullScreenCommand { get; private set; }
    public ReactiveCommand<TapeLoadingSpeed, Unit> SetTapeLoadSpeedCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> HelpKeyboardCommand { get; private set; }

    public MainWindowViewModel()
    {
        _statusBarTimer = new Timer(TimeSpan.FromSeconds(1));
        _statusBarTimer.AutoReset = true;
        _statusBarTimer.Elapsed += StatusBarTimerOnElapsed;

        var emulatorNotNull = this.WhenAnyValue(x => x.Emulator).Select(emulator => emulator is null);

        WindowOpenedCommand = ReactiveCommand.CreateFromTask(WindowOpenedAsync);
        WindowClosingCommand = ReactiveCommand.CreateFromTask(WindowClosingAsync);
        KeyDownCommand = ReactiveCommand.Create<KeyEventArgs>(HandleKeyDown);
        KeyUpCommand = ReactiveCommand.Create<KeyEventArgs>(HandleKeyUp);
        LoadFileCommand = ReactiveCommand.Create(HandleLoadFileAsync);
        SaveFileCommand = ReactiveCommand.Create(HandleSaveFileAsync);
        ChangeBorderSizeCommand = ReactiveCommand.Create<BorderSize>(HandleChangeBorderSize);
        ChangeRomCommand = ReactiveCommand.Create<RomType>(HandleChangeRom);
        ChangeComputerType = ReactiveCommand.Create<ComputerType>(HandleChangeComputerType);
        ChangeJoystickType = ReactiveCommand.Create<JoystickType>(HandleChangeJoystickType);
        ToggleUlaPlus = ReactiveCommand.Create(HandleToggleUlaPlus);
        PauseCommand = ReactiveCommand.Create(HandleMachinePause, emulatorNotNull);
        ResetCommand = ReactiveCommand.Create(HandleMachineReset, emulatorNotNull);
        SetEmulationSpeedCommand = ReactiveCommand.Create<string>(HandleSetEmulationSpeed);
        ToggleFullScreenCommand = ReactiveCommand.Create(HandleToggleFullScreen);
        SetTapeLoadSpeedCommand = ReactiveCommand.Create<TapeLoadingSpeed>(HandleSetTapeLoadingSpeed);
        HelpKeyboardCommand = ReactiveCommand.Create(HandleHelpKeyboardCommand);

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

    private void EmulatorOnRenderScreen(FrameBuffer framebuffer)
    {
        // If we are running in accelerated mode, we don't need to exceed 100 FPS
        if (_renderStopwatch.Elapsed - _lastScreenRender < TimeSpan.FromMilliseconds(8))
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            _frameBufferConverter.UpdateBitmap(framebuffer);
            ScreenControl.InvalidateVisual();
        });

        Interlocked.Increment(ref _frameCount);

        _lastScreenRender = _renderStopwatch.Elapsed;
    }

    private async Task WindowOpenedAsync()
    {
        _defaultSettings = await SettingsManager.LoadAsync<DefaultSettings>();

        HandleChangeBorderSize(_defaultSettings.BorderSize);
        ComputerType = _defaultSettings.ComputerType;
        IsUlaPlusEnabled = _defaultSettings.IsUlaPlusEnabled;
        RomType = _defaultSettings.RomType == RomType.Custom ? RomType.Original : _defaultSettings.RomType;
        JoystickType = _defaultSettings.JoystickType;
        TapeLoadingSpeed = _defaultSettings.TapeLoadingSpeed;

        CreateNewEmulator();
    }

    private async Task WindowClosingAsync()
    {
        _defaultSettings.BorderSize = BorderSize;
        _defaultSettings.ComputerType = ComputerType;
        _defaultSettings.IsUlaPlusEnabled = IsUlaPlusEnabled;
        _defaultSettings.RomType = RomType;
        _defaultSettings.JoystickType = JoystickType;
        _defaultSettings.TapeLoadingSpeed = TapeLoadingSpeed;

        await SettingsManager.SaveAsync(_defaultSettings);
    }

    private void CreateNewEmulator() =>
        InitializeEmulator(EmulatorFactory.Create(ComputerType, RomType));

    private void InitializeEmulator(Emulator emulator)
    {
        Emulator?.Stop();

        Emulator = emulator;

        Emulator.IsUlaPlusEnabled = IsUlaPlusEnabled;
        Emulator.TapeLoadingSpeed = TapeLoadingSpeed;
        Emulator.JoystickManager.SetupJoystick(JoystickType);
        Emulator.RenderScreen += EmulatorOnRenderScreen;

        TapeMenuViewModel.TapeManager = Emulator.TapeManager;

        _renderStopwatch.Restart();
        _lastScreenRender = TimeSpan.Zero;

        Emulator.Start();

        _statusBarTimer.Start();
    }

    private async Task HandleLoadFileAsync()
    {
        var files = await FileDialogs.OpenAnyFileAsync();
        if (files.Count <= 0)
        {
            return;
        }

        Emulator?.Pause();

        try
        {
            var fileType = FileTypeHelper.GetFileType(files[0].Path.LocalPath);
            if (fileType.IsSnapshot())
            {
                var emulator = SnapshotLoader.Load(files[0].Path.LocalPath);

                ComputerType = emulator.ComputerType;
                RomType = emulator.RomType;
                JoystickType = emulator.JoystickManager.JoystickType;

                InitializeEmulator(emulator);
            }
            else
            {
                Emulator?.LoadTape(files[0].Path.LocalPath);
            }
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

    private async Task HandleSaveFileAsync()
    {
        try
        {
            Emulator?.Pause();

            var file = await FileDialogs.SaveSnapshotFileAsync();
            if (file == null)
            {
                return;
            }
        }
        finally
        {
            Emulator?.Resume();
        }
    }

    private void HandleChangeBorderSize(BorderSize borderSize)
    {
        BorderSize = borderSize;

        _frameBufferConverter.SetBorderSize(borderSize);
        SpectrumScreen = _frameBufferConverter.Bitmap;
    }

    private void HandleChangeRom(RomType romType)
    {
        RomType = romType;

        if (Emulator != null)
        {
            Emulator.Stop();
            Emulator.RenderScreen -= EmulatorOnRenderScreen;
        }

        CreateNewEmulator();
    }

    private void HandleChangeComputerType(ComputerType computerType)
    {
        ComputerType = computerType;

        Emulator?.Stop();
        CreateNewEmulator();
    }

    private void HandleChangeJoystickType(JoystickType joystickType)
    {
        JoystickType = joystickType;
        Emulator?.JoystickManager.SetupJoystick(joystickType);
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

    private void HandleSetEmulationSpeed(string emulationSpeed)
    {
        if (!int.TryParse(emulationSpeed, out var emulationSpeedValue))
        {
            return;
        }

        Emulator?.SetEmulationSpeed(emulationSpeedValue);
        EmulationSpeed = emulationSpeed;
    }

    private void HandleToggleFullScreen() =>
        WindowState = WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;

    private void HandleSetTapeLoadingSpeed(TapeLoadingSpeed tapeLoadingSpeed) => TapeLoadingSpeed = tapeLoadingSpeed;

    private void HandleHelpKeyboardCommand()
    {
        if (_helpKeyboardView == null)
        {
            _helpKeyboardView = new HelpKeyboardView();
            _helpKeyboardView.Closed += (_, _) => _helpKeyboardView = null;

            if (MainWindow != null)
            {
                _helpKeyboardView.Show(MainWindow);
            }
            else
            {
                _helpKeyboardView.Show();
            }
        }
        else
        {
            _helpKeyboardView.Activate();
        }
    }

    private void HandleKeyUp(KeyEventArgs e)
    {
        if (JoystickType != JoystickType.None)
        {
            var joystickKeys = KeyMappings.ToJoystickAction(e);
            if (joystickKeys != JoystickInput.None)
            {
                Emulator?.JoystickManager.HandleInput(joystickKeys, isOn: false);
                return;
            }
        }

        var keys = KeyMappings.ToSpectrumKey(e);
        Emulator?.KeyboardHandler.HandleKeyUp(keys);
    }

    private void HandleKeyDown(KeyEventArgs e)
    {
        if (JoystickType != JoystickType.None)
        {
            var joystickKeys = KeyMappings.ToJoystickAction(e);
            if (joystickKeys != JoystickInput.None)
            {
                Emulator?.JoystickManager.HandleInput(joystickKeys, isOn: true);
                return;
            }
        }

        var keys = KeyMappings.ToSpectrumKey(e);
        Emulator?.KeyboardHandler.HandleKeyDown(keys);
    }

    private BorderSize _borderSize = BorderSize.Medium;
    public BorderSize BorderSize
    {
        get => _borderSize;
        set => this.RaiseAndSetIfChanged(ref _borderSize, value);
    }

    private RomType _romType = RomType.Original;
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

    private JoystickType _joystickType = JoystickType.None;
    public JoystickType JoystickType
    {
        get => _joystickType;
        set => this.RaiseAndSetIfChanged(ref _joystickType, value);
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

    private string _emulationSpeed = "100";
    public string EmulationSpeed
    {
        get => _emulationSpeed;
        set => this.RaiseAndSetIfChanged(ref _emulationSpeed, value);
    }

    private WindowState _windowState = WindowState.Normal;
    public WindowState WindowState
    {
        get => _windowState;
        set
        {
            WindowStateCommandName = value == WindowState.FullScreen ? "Exit Full Screen" : "Enter Full Screen";
            this.RaiseAndSetIfChanged(ref _windowState, value);
        }
    }

    private string _windowStateCommandName = "Enter Full Screen";
    public string WindowStateCommandName
    {
        get => _windowStateCommandName;
        set => this.RaiseAndSetIfChanged(ref _windowStateCommandName, value);
    }

    private TapeLoadingSpeed _tapeLoadingSpeed = TapeLoadingSpeed.Instant;
    public TapeLoadingSpeed TapeLoadingSpeed
    {
        get => _tapeLoadingSpeed;
        set
        {
            this.RaiseAndSetIfChanged(ref _tapeLoadingSpeed, value);
            if (Emulator != null)
            {
                Emulator.TapeLoadingSpeed = TapeLoadingSpeed;
            }
        }
    }
}
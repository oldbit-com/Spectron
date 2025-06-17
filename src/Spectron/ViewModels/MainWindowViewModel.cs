using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Debugger;
using OldBit.Spectron.Debugger.Messages;
using OldBit.Spectron.Debugger.ViewModels;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Emulation.Tape.Loader;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Files.Pok;
using OldBit.Spectron.Input;
using OldBit.Spectron.Messages;
using OldBit.Spectron.Models;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Theming;
using ComputerType = OldBit.Spectron.Emulation.ComputerType;
using JoystickType = OldBit.Spectron.Emulation.Devices.Joystick.JoystickType;
using MouseType = OldBit.Spectron.Emulation.Devices.Mouse.MouseType;

namespace OldBit.Spectron.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private const string DefaultTitle = "Spectron - ZX Spectrum Emulator";

    private readonly EmulatorFactory _emulatorFactory;
    private readonly TimeMachine _timeMachine;
    private readonly GamepadManager _gamepadManager;

    private readonly SnapshotManager _snapshotManager;
    private readonly StateManager _stateManager;
    private readonly Loader _loader;

    private readonly PreferencesService _preferencesService;
    private readonly SessionService _sessionService;
    private readonly DebuggerContext _debuggerContext;
    private readonly QuickSaveService _quickSaveService;
    private readonly ILogger _logger;
    private readonly FrameBufferConverter _frameBufferConverter = new(4, 4);
    private readonly KeyboardHook _keyboardHook;
    private readonly Stopwatch _renderStopwatch = new();
    private readonly FrameRateCalculator _frameRateCalculator = new();

    private Emulator? Emulator { get; set; }
    private Preferences _preferences = new();
    private TimeSpan _lastScreenRender = TimeSpan.Zero;
    private MediaRecorder? _mediaRecorder;
    private bool _canClose;
    private bool _isTimeMachineOpen;
    private DebuggerViewModel? _debuggerViewModel;
    private PokeFile? _pokeFile;
    private MouseHelper? _mouseHelper;
    private readonly ScreenshotViewModel _screenshotViewModel = new();

    public Control ScreenControl { get; set; } = null!;
    public Window? MainWindow { get; set; }
    public WindowNotificationManager NotificationManager { get; set; } = null!;

    public StatusBarViewModel StatusBarViewModel { get; } = new();
    public TapeMenuViewModel TapeMenuViewModel { get; }
    public RecentFilesViewModel RecentFilesViewModel { get; }

    #region Observable properties
    [ObservableProperty]
    private BorderSize _borderSize = BorderSize.Medium;

    [ObservableProperty]
    private RomType _romType = RomType.Original;

    [ObservableProperty]
    private ComputerType _computerType = ComputerType.Spectrum48K;

    [ObservableProperty]
    private JoystickType _joystickType = JoystickType.None;

    [ObservableProperty]
    private MouseType _mouseType = MouseType.None;

    [ObservableProperty]
    private bool _isUlaPlusEnabled;

    [ObservableProperty]
    private WriteableBitmap? _spectrumScreen;

    [ObservableProperty]
    private bool _isPaused;

    [ObservableProperty]
    private bool _isPauseOverlayVisible;

    [ObservableProperty]
    private bool _isTimeMachineCountdownVisible;

    [ObservableProperty]
    private string _emulationSpeed = "100";

    [ObservableProperty]
    private WindowState _windowState = WindowState.Normal;

    [ObservableProperty]
    private TapeSpeed _tapeLoadSpeed = TapeSpeed.Normal;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private bool _isTimeMachineEnabled;

    [ObservableProperty]
    private string _title = DefaultTitle;

    [ObservableProperty]
    private RecordingStatus _recordingStatus = RecordingStatus.None;

    [ObservableProperty]
    private int _timeMachineCountdownSeconds;

    [ObservableProperty]
    private Cursor _mouseCursor = Cursor.Default;
    #endregion

    #region Relay commands
    [RelayCommand]
    private async Task WindowOpened() => await WindowOpenedAsync();

    [RelayCommand]
    private async Task WindowClosing(WindowClosingEventArgs e) => await WindowClosingAsync(e);

    [RelayCommand]
    private void KeyDown(KeyEventArgs e) => HandleKeyDown(e);

    [RelayCommand]
    private void TimeMachineResumeEmulator() => HandleTimeMachineResumeEmulator();

    // File
    [RelayCommand]
    private async Task LoadFile() => await HandleLoadFileAsync();

    [RelayCommand]
    private async Task SaveFile() => await HandleSaveFileAsync();

    [RelayCommand]
    private void QuickSave() => HandleQuickSave();

    [RelayCommand]
    private void QuickLoad() => HandleQuickLoad();

    [RelayCommand]
    private async Task ShowPreferencesView() => await OpenPreferencesWindow();

    [RelayCommand]
    private void ExitApplication() => MainWindow?.Close();

    // Machine
    [RelayCommand]
    public void ChangeComputerType(ComputerType computerType) => HandleChangeComputerType(computerType);

    [RelayCommand]
    private async Task ChangeRom(RomType romType) => await HandleChangeRomAsync(romType);

    [RelayCommand]
    public void ChangeJoystickType(JoystickType joystickType) => JoystickType = joystickType;

    [RelayCommand]
    public void ChangeMouseType(MouseType mouseType) => MouseType = mouseType;

    [RelayCommand]
    public void ToggleUlaPlus() => IsUlaPlusEnabled = !IsUlaPlusEnabled;

    // Control
    [RelayCommand]
    private void SetEmulationSpeed(string emulationSpeed) => HandleSetEmulationSpeed(emulationSpeed);

    [RelayCommand]
    private void TogglePause() => HandleTogglePause();

    [RelayCommand(CanExecute = nameof(IsTimeMachineEnabled))]
    private async Task ShowTimeMachineView() => await OpenTimeMachineWindow();

    [RelayCommand]
    private void ToggleMute() => HandleToggleMute();

    [RelayCommand]
    private void TriggerNmi() => Emulator?.RequestNmi();

    [RelayCommand]
    private void Reset() => HandleMachineReset();

    [RelayCommand]
    private void HardReset() => HandleMachineReset(hardReset: true);

    // Tools
    [RelayCommand]
    private void ShowDebuggerView() => OpenDebuggerWindow();

    [RelayCommand]
    private async Task StartAudioRecording() => await HandleStartAudioRecordingAsync();

    [RelayCommand]
    private async Task StartVideoRecording() => await HandleStartVideoRecordingAsync();

    [RelayCommand]
    private void StopRecording() => HandleStopRecording();

    [RelayCommand]
    public void ShowScreenshotViewer() => OpenScreenshotViewer();

    [RelayCommand]
    private void TakeScreenshot() => _screenshotViewModel.AddScreenshot(SpectrumScreen);

    // View
    [RelayCommand]
    private void ToggleFullScreen() => HandleToggleFullScreen();

    [RelayCommand]
    private void ChangeBorderSize(BorderSize borderSize) => HandleChangeBorderSize(borderSize);

    [RelayCommand]
    private void ShowTrainers() => OpenTrainersWindow();

    [RelayCommand]
    private void ShowPrintOutput() => OpenPrintOutputViewer();

    // Tape
    [RelayCommand]
    private void SetTapeLoadSpeed(TapeSpeed tapeSpeed) => HandleSetTapeLoadingSpeed(tapeSpeed);

    // Help
    [RelayCommand]
    private void ShowAboutView() => OpenAboutWindow();

    [RelayCommand]
    private void ShowKeyboardHelpView() => ShowKeyboardHelpWindow();
    #endregion

    public MainWindowViewModel(
        EmulatorFactory emulatorFactory,
        TimeMachine timeMachine,
        GamepadManager gamepadManager,
        SnapshotManager snapshotManager,
        StateManager stateManager,
        Loader loader,
        PreferencesService preferencesService,
        SessionService sessionService,
        RecentFilesViewModel recentFilesViewModel,
        TapeMenuViewModel tapeMenuViewModel,
        DebuggerContext debuggerContext,
        TapeManager tapeManager,
        QuickSaveService quickSaveService,
        ILogger<MainWindowViewModel> logger)
    {
        _emulatorFactory = emulatorFactory;
        _timeMachine = timeMachine;
        _gamepadManager = gamepadManager;
        _snapshotManager = snapshotManager;
        _stateManager = stateManager;
        _loader = loader;
        _preferencesService = preferencesService;
        _sessionService = sessionService;
        _debuggerContext = debuggerContext;
        _quickSaveService = quickSaveService;
        _logger = logger;

        RecentFilesViewModel = recentFilesViewModel;
        TapeMenuViewModel = tapeMenuViewModel;
        recentFilesViewModel.OpenRecentFileAsync = async fileName => await HandleLoadFileAsync(fileName);

        SpectrumScreen = _frameBufferConverter.ScreenBitmap;

        tapeManager.TapeStateChanged += HandleTapeStateChanged;

        _keyboardHook = new KeyboardHook();
        _keyboardHook.SpectrumKeyPressed  += HandleSpectrumKeyPressed;
        _keyboardHook.SpectrumKeyReleased += HandleSpectrumKeyReleased;
        _keyboardHook.Run();

        WeakReferenceMessenger.Default.Register<ResetEmulatorMessage>(this, (_, message) =>
            HandleMachineReset(message.HardReset));

        _frameRateCalculator.FrameRateChanged = fps =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                StatusBarViewModel.FramesPerSecond = fps.ToString();

                var tapeBlockProgress = string.Empty;
                if (Emulator?.TapeManager.BlockReadProgressPercentage > 0)
                {
                    tapeBlockProgress = $"{(int)Emulator.TapeManager.BlockReadProgressPercentage}%";
                }

                StatusBarViewModel.TapeLoadProgress = tapeBlockProgress;
            });
        };
        _frameRateCalculator.Start();

        StatusBarViewModel.ComputerType = ComputerType;
    }

    partial void OnTapeLoadSpeedChanged(TapeSpeed value) =>
        Emulator?.SetTapeLoadingSpeed(TapeLoadSpeed);

    partial void OnIsTimeMachineEnabledChanged(bool value)
    {
        _timeMachine.IsEnabled = value;
        NotifyCanExecuteChanged(nameof(ShowTimeMachineViewCommand));
    }

    partial void OnRecordingStatusChanged(RecordingStatus value) =>
        StatusBarViewModel.RecordingStatus = RecordingStatus;

    partial void OnIsPausedChanged(bool value) =>
        _debuggerViewModel?.HandlePause(value);

    private void NotifyCanExecuteChanged(string commandName)
    {
        var command = GetType().GetProperty(commandName)?.GetValue(this) as IRelayCommand;
        command?.NotifyCanExecuteChanged();
    }

    public void OnViewClosed(object? viewModel)
    {
        if (viewModel is not DebuggerViewModel)
        {
            return;
        }

        Resume();
        _debuggerViewModel = null;
    }

    private static void OpenAboutWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowAboutViewMessage());

    private static void ShowKeyboardHelpWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowKeyboardViewMessage());

    private static void OpenScreenshotViewer() =>
        WeakReferenceMessenger.Default.Send(new ShowScreenshotViewMessage());

    private void OpenTrainersWindow() =>
        WeakReferenceMessenger.Default.Send(new ShowTrainerViewMessage(Emulator!, _pokeFile));

    private void OpenPrintOutputViewer() =>
        WeakReferenceMessenger.Default.Send(new ShowPrintOutputViewMessage(Emulator!.Printer));

    private void OpenDebuggerWindow()
    {
        _debuggerViewModel = new DebuggerViewModel(_debuggerContext, Emulator!, _preferences.Debugger);

        if (!IsPaused)
        {
            Pause(showOverlay: false);
        }

        WeakReferenceMessenger.Default.Send(new ShowDebuggerViewMessage(_debuggerViewModel));
    }

    public async Task OpenPreferencesWindow()
    {
        var resumeAfter = false;

        if (!IsPaused)
        {
            Pause();
            resumeAfter = true;
        }

        var preferences = await WeakReferenceMessenger.Default.Send(new ShowPreferencesViewMessage(_preferences, _gamepadManager));

        ThemeManager.SelectTheme(preferences?.Theme ?? _preferences.Theme);

        if (preferences != null)
        {
            _preferences = preferences;

            IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
            Emulator.SetUlaPlus(IsUlaPlusEnabled);

            TapeLoadSpeed = preferences.Tape.LoadSpeed;
            Emulator.SetTapeSettings(_preferences.Tape);

            IsTimeMachineEnabled = preferences.TimeMachine.IsEnabled;
            _timeMachine.SnapshotInterval = preferences.TimeMachine.SnapshotInterval;
            _timeMachine.MaxDuration = preferences.TimeMachine.MaxDuration;
            TimeMachineCountdownSeconds = preferences.TimeMachine.CountdownSeconds;

            JoystickType = preferences.Joystick.JoystickType;
            MouseType = preferences.Mouse.MouseType;
            SetMouseCursor();

            ConfigureEmulatorSettings();
        }

        if (resumeAfter)
        {
            Resume();
        }
    }

    private async Task OpenTimeMachineWindow()
    {
        if (!_preferences.TimeMachine.IsEnabled || _isTimeMachineOpen)
        {
            return;
        }

        var resumeAfter = false;

        if (!IsPaused)
        {
            Pause();
            resumeAfter = true;
        }

        _isTimeMachineOpen = true;

        var viewModel = new TimeMachineViewModel(_timeMachine, Emulator!.JoystickManager, Emulator.CommandManager, _logger);

        var entry = await WeakReferenceMessenger.Default.Send(new ShowTimeMachineViewMessage(viewModel));

        if (entry != null)
        {
            var snapshot = entry.GetSnapshot();

            if (snapshot == null)
            {
                _logger.LogError("Failed to get snapshot from time machine entry");
                return;
            }

            CreateEmulator(snapshot);
        }

        _isTimeMachineOpen = false;

        if (resumeAfter)
        {
            Resume();
        }
    }

    private void EmulatorFrameCompleted(FrameBuffer frameBuffer, AudioBuffer audioBuffer)
    {
        // Keep max 50 FPS
        if (_renderStopwatch.Elapsed - _lastScreenRender < TimeSpan.FromMilliseconds(19))
        {
            return;
        }

        _lastScreenRender = _renderStopwatch.Elapsed;
        _frameRateCalculator.FrameCompleted();

        Dispatcher.UIThread.Post(() =>
        {
            _frameBufferConverter.UpdateBitmap(frameBuffer);

            ScreenControl.InvalidateVisual();
        });

        _mediaRecorder?.AppendFrame(frameBuffer, audioBuffer);

        if (_quickSaveService.QuickSaveIfRequested(Emulator))
        {
            StatusBarViewModel.AnimateQuickSave();
        }
    }

    private async Task WindowOpenedAsync()
    {
        _preferences = await _preferencesService.LoadAsync();

        ThemeManager.SelectTheme(_preferences.Theme);

        IsMuted = _preferences.Audio.IsMuted;

        IsTimeMachineEnabled = _preferences.TimeMachine.IsEnabled;
        _timeMachine.SnapshotInterval = _preferences.TimeMachine.SnapshotInterval;
        _timeMachine.MaxDuration = _preferences.TimeMachine.MaxDuration;
        TimeMachineCountdownSeconds = _preferences.TimeMachine.CountdownSeconds;

        await RecentFilesViewModel.LoadAsync();

        HandleChangeBorderSize(_preferences.BorderSize);

        if (_preferences.Resume.IsResumeEnabled)
        {
            var snapshot = await _sessionService.LoadAsync();

            if (snapshot != null)
            {
                CreateEmulator(snapshot);
                UpdateWindowTitle();
            }
        }

        if (Emulator == null)
        {
            CreateEmulator(_preferences.ComputerType, _preferences.RomType);
        }

        TapeLoadSpeed = _preferences.Tape.LoadSpeed;
        Emulator?.SetTapeSettings(_preferences.Tape);
    }

    private async Task WindowClosingAsync(WindowClosingEventArgs args)
    {
        if (_canClose)
        {
            return;
        }

        args.Cancel = true;

        Emulator?.Shutdown(isAppClosing: true);
        _keyboardHook?.Dispose();
        _frameRateCalculator.Dispose();

        _preferences.Audio.IsMuted = IsMuted;
        _preferences.BorderSize = BorderSize;

        await Task.WhenAll(
            _preferencesService.SaveAsync(_preferences),
            RecentFilesViewModel.SaveAsync(),
            _sessionService.SaveAsync(Emulator, _preferences.Resume));

        _canClose = true;
        MainWindow?.Close();
    }

    private void CreateEmulator(ComputerType computerType, RomType romType, byte[]? customRom = null)
    {
        var emulator = _emulatorFactory.Create(computerType, romType, customRom);

        emulator.SetUlaPlus(_preferences.IsUlaPlusEnabled);
        emulator.MouseManager.SetupMouse(_preferences.Mouse.MouseType);
        _mouseHelper = new MouseHelper(emulator.MouseManager);
        emulator.JoystickManager.SetupJoystick(_preferences.Joystick.JoystickType);

        InitializeEmulator(emulator);
    }

    private void CreateEmulator(StateSnapshot stateSnapshot)
    {
        Emulator?.Reset();

        var emulator = _stateManager.CreateEmulator(stateSnapshot);
        _mouseHelper = new MouseHelper(emulator.MouseManager);

        InitializeEmulator(emulator);
    }

    private bool CreateEmulator(Stream stream, FileType fileType)
    {
        Emulator? emulator = null;

        if (fileType.IsSnapshot())
        {
            emulator = _snapshotManager.Load(stream, fileType);
        }
        else if (fileType.IsTape())
        {
            emulator = _loader.EnterLoadCommand(ComputerType);
            emulator.TapeManager.InsertTape(stream, fileType,
                _preferences.Tape.IsAutoPlayEnabled && TapeLoadSpeed != TapeSpeed.Instant);

            emulator.SetUlaPlus(_preferences.IsUlaPlusEnabled);
            emulator.MouseManager.SetupMouse(_preferences.Mouse.MouseType);
            _mouseHelper = new MouseHelper(emulator.MouseManager);
            emulator.JoystickManager.SetupJoystick(_preferences.Joystick.JoystickType);
        }

        if (emulator != null)
        {
            InitializeEmulator(emulator);
        }

        return emulator != null;
    }

    private void InitializeEmulator(Emulator emulator)
    {
        ShutdownEmulator();

        Emulator = emulator;
        IsPaused = false;

        ComputerType = Emulator.ComputerType;
        RomType = Emulator.RomType;
        JoystickType = Emulator.JoystickManager.JoystickType;
        MouseType = Emulator.MouseManager.MouseType;
        IsUlaPlusEnabled = Emulator.IsUlaPlusEnabled;

        Emulator.TapeLoadSpeed = TapeLoadSpeed;
        Emulator.FrameCompleted += EmulatorFrameCompleted;

        ConfigureEmulatorSettings();

        if (IsMuted)
        {
            Emulator.AudioManager.Mute();
        }

        _renderStopwatch.Restart();
        _lastScreenRender = TimeSpan.Zero;

        Emulator.CommandManager.CommandReceived += CommandManagerOnCommandReceived;

        _debuggerViewModel?.ConfigureEmulator(Emulator);

        Emulator.Start();
    }

    private void ConfigureEmulatorSettings()
    {
        Emulator.SetFloatingBusSupport(_preferences.IsFloatingBusEnabled);
        Emulator.SetAudioSettings(_preferences.Audio);
        Emulator.SetGamepad(_preferences.Joystick);
        Emulator.SetDivMMc(_preferences.DivMmc);
        Emulator.SetPrinter(_preferences.Printer);

        StatusBarViewModel.IsDivMmcEnabled = _preferences.DivMmc.IsEnabled;
        StatusBarViewModel.IsPrinterEnabled = _preferences.Printer.IsZxPrinterEnabled;
        StatusBarViewModel.IsUlaPlusEnabled = IsUlaPlusEnabled;
        StatusBarViewModel.IsTapeLoaded = Emulator!.TapeManager.IsTapeLoaded;
        StatusBarViewModel.TapeLoadProgress = string.Empty;
    }

    private void CommandManagerOnCommandReceived(object? sender, CommandEventArgs e)
    {
        if (e.Command is not GamepadActionCommand gamepadCommand)
        {
            return;
        }

        if (gamepadCommand.State == InputState.Pressed)
        {
            return;
        }

        if (MainWindow?.IsActive != true)
        {
            return;
        }

        switch (gamepadCommand.Action)
        {
            case GamepadAction.Pause:
                HandleTogglePause();
                break;

            case GamepadAction.TimeTravel:
                Dispatcher.UIThread.InvokeAsync(async () => await OpenTimeMachineWindow());
                break;

            case GamepadAction.QuickSave:
                HandleQuickSave();
                break;

            case GamepadAction.QuickLoad:
                HandleQuickLoad();
                break;

            case GamepadAction.NMI:
                Emulator?.RequestNmi();
                break;
        }
    }

    private void ShutdownEmulator()
    {
        if (Emulator == null)
        {
            return;
        }

        Emulator.Shutdown();
        Emulator.FrameCompleted -= EmulatorFrameCompleted;
        Emulator.CommandManager.CommandReceived -= CommandManagerOnCommandReceived;

        Emulator = null;
    }

    private void UpdateWindowTitle()
    {
        if (RecentFilesViewModel.CurrentFileName == string.Empty)
        {
            Title = DefaultTitle;

            return;
        }

        Title = $"{DefaultTitle} [{RecentFilesViewModel.CurrentFileName}]";
    }
}
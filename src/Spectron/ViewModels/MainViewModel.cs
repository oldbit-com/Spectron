using System;
using System.Diagnostics;
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
using OldBit.Spectron.CommandLine;
using OldBit.Spectron.Controls;
using OldBit.Spectron.Debugger;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Messages;
using OldBit.Spectron.Debugger.ViewModels;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Rzx;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Emulation.Tape.Loader;
using OldBit.Spectron.Emulation.TimeTravel;
using OldBit.Spectron.Files.Pok;
using OldBit.Spectron.Input;
using OldBit.Spectron.Logging;
using OldBit.Spectron.Messages;
using OldBit.Spectron.Models;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;
using ComputerType = OldBit.Spectron.Emulation.ComputerType;
using JoystickType = OldBit.Spectron.Emulation.Devices.Joystick.JoystickType;
using MouseType = OldBit.Spectron.Emulation.Devices.Mouse.MouseType;

namespace OldBit.Spectron.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string DefaultTitle = "Spectron - ZX Spectrum Emulator";

    private readonly EmulatorFactory _emulatorFactory;
    private readonly FileDialogs _fileDialogs;
    private readonly IMessageDialogs _messageDialogs;
    private readonly TimeMachine _timeMachine;
    private readonly GamepadManager _gamepadManager;

    private readonly SnapshotManager _snapshotManager;
    private readonly StateSnapshotManager _stateSnapshotManager;
    private readonly Loader _loader;

    private readonly PreferencesService _preferencesService;
    private readonly FavoritesService _favoritesService;
    private readonly SessionService _sessionService;
    private readonly DebuggerContext _debuggerContext;
    private readonly QuickSaveService _quickSaveService;
    private readonly ILogStore _logStore;
    private readonly ILogger _logger;
    private readonly KeyboardHandler _keyboardHandler;
    private readonly Stopwatch _renderStopwatch = new();
    private readonly FrameRateCalculator _frameRateCalculator = new();
    private readonly ScreenshotViewModel _screenshotViewModel;

    private Emulator? Emulator { get; set; }
    private FrameBufferConverter? _frameBufferConverter;
    private Preferences _preferences = new();
    private FavoritePrograms _favorites = new();
    private TimeSpan _lastScreenRender = TimeSpan.Zero;
    private MediaRecorder? _mediaRecorder;
    private bool _canClose;
    private bool _isTimeMachineOpen;
    private DebuggerViewModel? _debuggerViewModel;
    private PokeFile? _pokeFile;
    private MouseInputHandler? _mouseInputHandler;
    private BreakpointHandler? _breakpointHandler;
    private RzxController? _rzxController;

    public Image ScreenControl { get; set; } = null!;
    public Window? MainWindow { get; set; }
    public MainMenu? MainMenu { get; set; }
    public CommandLineArgs? CommandLineArgs { get; set; }
    public WindowNotificationManager NotificationManager { get; set; } = null!;

    public StatusBarViewModel StatusBarViewModel { get; } = new();
    public TapeMenuViewModel TapeMenuViewModel { get; }
    public MicrodriveMenuViewModel MicrodriveMenuViewModel { get; }
    public DiskDriveMenuViewModel DiskDriveMenuViewModel { get; }
    public RecentFilesViewModel RecentFilesViewModel { get; }
    public FavoritesViewModel FavoritesViewModel { get; }

    #region Observable properties
    [ObservableProperty]
    public partial bool IsNativeMenuEnabled { get; private set; }

    [ObservableProperty]
    public partial bool IsMenuVisible { get; set; } = true;

    [ObservableProperty]
    public partial BorderSize BorderSize { get; set; } = BorderSize.Medium;

    [ObservableProperty]
    public partial ScreenEffect ScreenEffect { get; set; } = ScreenEffect.None;

    [ObservableProperty]
    public partial RomType RomType { get; set; } = RomType.Original;

    [ObservableProperty]
    public partial ComputerType ComputerType { get; set; } = ComputerType.Spectrum48K;

    [ObservableProperty]
    public partial JoystickType JoystickType { get; set; } = JoystickType.None;

    [ObservableProperty]
    public partial MouseType MouseType { get; set; } = MouseType.None;

    [ObservableProperty]
    public partial bool IsUlaPlusEnabled { get; set; }

    [ObservableProperty]
    public partial WriteableBitmap? SpectrumScreen { get; set; }

    [ObservableProperty]
    public partial bool IsPaused { get; set; }

    [ObservableProperty]
    public partial bool IsPauseOverlayVisible { get; set; }

    [ObservableProperty]
    public partial bool IsTimeMachineCountdownVisible { get; set; }

    [ObservableProperty]
    public partial string EmulationSpeed { get; set; } = "100";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFullScreen))]
    public partial WindowState WindowState { get; set; } = WindowState.Normal;

    public bool IsFullScreen => WindowState == WindowState.FullScreen;

    [ObservableProperty]
    public partial TapeSpeed TapeLoadSpeed { get; set; } = TapeSpeed.Normal;

    [ObservableProperty]
    public partial bool IsAudioMuted { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowTimeMachineViewCommand))]
    public partial bool IsTimeMachineEnabled { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = DefaultTitle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStartRecording))]
    [NotifyPropertyChangedFor(nameof(CanStopRecording))]
    [NotifyCanExecuteChangedFor(nameof(StartAudioRecordingCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartVideoRecordingCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopRecordingCommand))]
    public partial RecordingStatus RecordingStatus { get; set; } = RecordingStatus.None;

    public bool CanStartRecording => RecordingStatus == RecordingStatus.None;

    public bool CanStopRecording => RecordingStatus != RecordingStatus.None;

    [ObservableProperty]
    public partial int TimeMachineCountdownSeconds { get; set; }

    [ObservableProperty]
    public partial string MouseCursor { get; set; } = MouseCursors.Default;

    [ObservableProperty]
    public partial bool IsInterface1Enabled { get; set; }

    [ObservableProperty]
    public partial int NumberOfMicrodrives { get; set; }

    [ObservableProperty]
    public partial bool IsBeta128Enabled { get; set; }

    [ObservableProperty]
    public partial int NumberOfBeta128Drives { get; set; }
    #endregion

    #region Relay commands
    [RelayCommand]
    private async Task WindowOpened() => await WindowOpenedAsync();

    [RelayCommand]
    private async Task WindowClosing(WindowClosingEventArgs e) => await WindowClosingAsync(e);

    [RelayCommand]
    private void KeyDown(KeyEventArgs e) => HandleKeyDown(e);

    [RelayCommand]
    private void KeyUp(KeyEventArgs e) => HandleKeyUp(e);

    [RelayCommand]
    private void TimeMachineResumeEmulator() => Resume();

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
    [RelayCommand(CanExecute = nameof(CanStartRecording))]
    private async Task StartAudioRecording() => await HandleStartAudioRecordingAsync();

    [RelayCommand(CanExecute = nameof(CanStartRecording))]
    private async Task StartVideoRecording() => await HandleStartVideoRecordingAsync();

    [RelayCommand(CanExecute = nameof(CanStopRecording))]
    private void StopRecording() => HandleStopRecording();

    [RelayCommand]
    private void ShowScreenshotViewer() => OpenScreenshotViewer();

    [RelayCommand]
    private Task TakeScreenshot() => HandleScreenshot();

    // View
    [RelayCommand]
    private void ToggleFullScreen() => HandleToggleFullScreen();

    [RelayCommand]
    private void ChangeBorderSize(BorderSize borderSize) => HandleChangeBorderSize(borderSize);

    [RelayCommand]
    private void ChangeScreenEffect(ScreenEffect screenEffect) => HandleChangeScreenEffect(screenEffect);

    [RelayCommand]
    private void ShowTrainers() => OpenTrainersWindow();

    [RelayCommand]
    private void ShowPrintOutput() => OpenPrintOutputViewer();

    // Favorites
    [RelayCommand]
    private void ShowFavoritesView() => OpenFavoritesWindow();

    // Tape
    [RelayCommand]
    private void SetTapeLoadSpeed(TapeSpeed tapeSpeed) => HandleSetTapeLoadingSpeed(tapeSpeed);

    // Debug
    [RelayCommand]
    private void ShowDebuggerView() => OpenDebuggerWindow();

    [RelayCommand]
    private void ToggleBreakpoints() => BreakpointsEnabled = !BreakpointsEnabled;

    [RelayCommand]
    private void ShowMemoryView() => OpenMemoryWindow();

    // Help
    [RelayCommand]
    private static void ShowAboutView() => OpenAboutWindow();

    [RelayCommand]
    private void ShowKeyboardHelpView() => ShowKeyboardHelpWindow();

    [RelayCommand]
    private void ShowLogView() => ShowLogViewWindow();

    #endregion

    public MainViewModel(
        EmulatorFactory emulatorFactory,
        FileDialogs fileDialogs,
        IMessageDialogs messageDialogs,
        TimeMachine timeMachine,
        GamepadManager gamepadManager,
        SnapshotManager snapshotManager,
        StateSnapshotManager stateSnapshotManager,
        Loader loader,
        PreferencesService preferencesService,
        FavoritesService favoritesService,
        SessionService sessionService,
        RecentFilesViewModel recentFilesViewModel,
        FavoritesViewModel favoritesViewModel,
        TapeMenuViewModel tapeMenuViewModel,
        MicrodriveMenuViewModel microdriveMenuViewModel,
        DiskDriveMenuViewModel diskDriveMenuViewModel,
        DebuggerContext debuggerContext,
        TapeManager tapeManager,
        MicrodriveManager microdriveManager,
        DiskDriveManager diskDriveManager,
        QuickSaveService quickSaveService,
        ILogStore logStore,
        ILogger<MainViewModel> logger)
    {
        _emulatorFactory = emulatorFactory;
        _fileDialogs = fileDialogs;
        _messageDialogs = messageDialogs;
        _timeMachine = timeMachine;
        _gamepadManager = gamepadManager;
        _snapshotManager = snapshotManager;
        _stateSnapshotManager = stateSnapshotManager;
        _loader = loader;
        _preferencesService = preferencesService;
        _favoritesService = favoritesService;
        _sessionService = sessionService;
        _debuggerContext = debuggerContext;
        _quickSaveService = quickSaveService;
        _logStore = logStore;
        _logger = logger;

        RecentFilesViewModel = recentFilesViewModel;
        FavoritesViewModel = favoritesViewModel;
        TapeMenuViewModel = tapeMenuViewModel;
        MicrodriveMenuViewModel = microdriveMenuViewModel;
        DiskDriveMenuViewModel = diskDriveMenuViewModel;
        recentFilesViewModel.OpenRecentFileAsync = async fileName => await HandleLoadFileAsync(fileName);
        _screenshotViewModel = new ScreenshotViewModel(fileDialogs, messageDialogs);

        tapeManager.TapeChanged += HandleTapeTapeChanged;
        microdriveManager.CartridgeChanged += HandleCartridgeChanged;
        diskDriveManager.DiskChanged += HandleFloppyDiskChanged;
        diskDriveManager.DiskActivity += HandleDiskActivity;

        _keyboardHandler = new KeyboardHandler();
        _keyboardHandler.SpectrumKeyPressed += HandleSpectrumKeyPressed;
        _keyboardHandler.SpectrumKeyReleased += HandleSpectrumKeyReleased;

        WeakReferenceMessenger.Default.Register<ResetEmulatorMessage>(this, (_, message) =>
            HandleMachineReset(message.HardReset));

        WeakReferenceMessenger.Default.Register<ResumeFromDebugMessage>(this, (_, _) =>
            ResumeFromDebug());

        WeakReferenceMessenger.Default.Register<PauseForDebugMessage>(this, (_, _) =>
            PauseForDebug());

        WeakReferenceMessenger.Default.Register<UpdateFavoritesMessage>(this, async void (_, message) =>
        {
            try
            {
                _favorites = message.Favorites;
                await _favoritesService.SaveAsync(_favorites);
            }
            catch
            {
                // Ignore
            }
        });

        WeakReferenceMessenger.Default.Register<OpenFavoriteMessage>(this, async void (_, message) =>
        {
            try
            {
                await OpenFavorite(message.Favorite);
            }
            catch
            {
                // Ignore
            }
        });

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
            _frameBufferConverter?.UpdateBitmap();
            ScreenControl.InvalidateVisual();
        });

        _mediaRecorder?.AppendFrame(frameBuffer, audioBuffer);

        if (_quickSaveService.QuickSaveIfRequested(Emulator))
        {
            StatusBarViewModel.AnimateQuickSave();
        }
    }

    public void OnViewClosed(Type? viewModel)
    {
        if (viewModel == typeof(DebuggerViewModel))
        {
            DebuggerWindowClosed();
        }
    }

    private void HandleToggleMute()
    {
        IsAudioMuted = !IsAudioMuted;

        if (IsAudioMuted)
        {
            Emulator?.AudioManager.Mute();
        }
        else
        {
            Emulator?.AudioManager.UnMute();
        }
    }

    private void ConfigureTimeMachine(TimeMachineSettings timeMachineSettings)
    {
        IsTimeMachineEnabled = timeMachineSettings.IsEnabled;
        _timeMachine.SnapshotInterval = timeMachineSettings.SnapshotInterval;
        _timeMachine.MaxDuration = timeMachineSettings.MaxDuration;
        TimeMachineCountdownSeconds = timeMachineSettings.CountdownSeconds;
    }
}
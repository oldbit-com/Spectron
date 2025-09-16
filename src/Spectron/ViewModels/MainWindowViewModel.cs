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
using OldBit.Spectron.Debugger;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Messages;
using OldBit.Spectron.Debugger.ViewModels;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrives;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Emulation.Tape.Loader;
using OldBit.Spectron.Files.Pok;
using OldBit.Spectron.Input;
using OldBit.Spectron.Models;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;
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
    private readonly ScreenshotViewModel _screenshotViewModel = new();

    private Emulator? Emulator { get; set; }
    private Preferences _preferences = new();
    private TimeSpan _lastScreenRender = TimeSpan.Zero;
    private MediaRecorder? _mediaRecorder;
    private bool _canClose;
    private bool _isTimeMachineOpen;
    private DebuggerViewModel? _debuggerViewModel;
    private PokeFile? _pokeFile;
    private MouseHelper? _mouseHelper;
    private BreakpointHandler? _breakpointHandler;

    public Control ScreenControl { get; set; } = null!;
    public Window? MainWindow { get; set; }
    public CommandLineArgs? CommandLineArgs { get; set; }
    public WindowNotificationManager NotificationManager { get; set; } = null!;

    public StatusBarViewModel StatusBarViewModel { get; } = new();
    public TapeMenuViewModel TapeMenuViewModel { get; }
    public MicrodriveMenuViewModel MicrodriveMenuViewModel { get; }
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
    private bool _isAudioMuted;

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
    
    [ObservableProperty]
    private bool _isInterface1Enabled;

    [ObservableProperty]
    private int _connectedMicrodrivesCount;
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

    // Debug
    [RelayCommand]
    private void ShowDebuggerView() => OpenDebuggerWindow();

    [RelayCommand]
    private void ToggleBreakpoints() => BreakpointsEnabled = !BreakpointsEnabled;

    // Help
    [RelayCommand]
    private static void ShowAboutView() => OpenAboutWindow();

    [RelayCommand]
    private static void ShowKeyboardHelpView() => ShowKeyboardHelpWindow();
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
        MicrodriveMenuViewModel microdriveMenuViewModel,
        DebuggerContext debuggerContext,
        TapeManager tapeManager,
        MicrodriveManager microdriveManager,
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
        MicrodriveMenuViewModel = microdriveMenuViewModel;
        recentFilesViewModel.OpenRecentFileAsync = async fileName => await HandleLoadFileAsync(fileName);

        SpectrumScreen = _frameBufferConverter.ScreenBitmap;

        tapeManager.TapeChanged += HandleTapeTapeChanged;
        microdriveManager.CartridgeChanged += HandleCartridgeChanged;

        _keyboardHook = new KeyboardHook();
        _keyboardHook.SpectrumKeyPressed  += HandleSpectrumKeyPressed;
        _keyboardHook.SpectrumKeyReleased += HandleSpectrumKeyReleased;
        _keyboardHook.Run();

        WeakReferenceMessenger.Default.Register<ResetEmulatorMessage>(this, (_, message) =>
            HandleMachineReset(message.HardReset));

        WeakReferenceMessenger.Default.Register<ResumeFromDebugMessage>(this, (_, _) =>
            ResumeFromDebug());

        WeakReferenceMessenger.Default.Register<PauseForDebugMessage>(this, (_, _) =>
            PauseForDebug());

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
            _frameBufferConverter.UpdateBitmap(frameBuffer);

            ScreenControl.InvalidateVisual();
        });

        _mediaRecorder?.AppendFrame(frameBuffer, audioBuffer);

        if (_quickSaveService.QuickSaveIfRequested(Emulator))
        {
            StatusBarViewModel.AnimateQuickSave();
        }
    }

    // TODO: This needs to be updated
    private void NotifyCanExecuteChanged(string commandName)
    {
        var command = GetType().GetProperty(commandName)?.GetValue(this) as IRelayCommand;
        command?.NotifyCanExecuteChanged();
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
}
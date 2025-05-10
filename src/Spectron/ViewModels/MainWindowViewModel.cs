using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Debugger;
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
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Theming;
using ReactiveUI;
using ComputerType = OldBit.Spectron.Emulation.ComputerType;
using JoystickType = OldBit.Spectron.Emulation.Devices.Joystick.JoystickType;
using MouseType = OldBit.Spectron.Emulation.Devices.Mouse.MouseType;
using Timer = System.Timers.Timer;

namespace OldBit.Spectron.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
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
    private readonly Timer _statusBarTimer;
    private readonly KeyboardHook _keyboardHook;
    private readonly Stopwatch _renderStopwatch = new();

    private Emulator? Emulator { get; set; }
    private Preferences _preferences = new();
    private int _frameCount;
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

    public ReactiveCommand<Unit, Unit> WindowOpenedCommand { get; private set; }
    public ReactiveCommand<WindowClosingEventArgs, Unit> WindowClosingCommand { get; private set; }

    public ReactiveCommand<KeyEventArgs, Unit> KeyDownCommand { get; private set; }

    public ReactiveCommand<Unit, Unit> TimeMachineResumeEmulatorCommand  { get; private set; }

    // File
    public ReactiveCommand<Unit, Task> LoadFileCommand { get; private set; }
    public ReactiveCommand<Unit, Task> SaveFileCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> QuickSaveCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> QuickLoadCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowPreferencesViewCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ExitApplicationCommand { get; private set; }

    // Emulator
    public ReactiveCommand<ComputerType, Unit> ChangeComputerType { get; private set; }
    public ReactiveCommand<RomType, Task> ChangeRomCommand { get; private set; }
    public ReactiveCommand<JoystickType, Unit> ChangeJoystickType { get; private set; }
    public ReactiveCommand<MouseType, Unit> ChangeMouseType { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleUlaPlus { get; private set; }

    // Control
    public ReactiveCommand<string, Unit> SetEmulationSpeedCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TogglePauseCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowTimeMachineViewCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleMuteCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TriggerNmiCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> HardResetCommand { get; private set; }

    // Tools
    public ReactiveCommand<Unit, Task> ShowDebuggerViewCommand { get; private set; }
    public ReactiveCommand<Unit, Task> StartAudioRecordingCommand { get; private set; }
    public ReactiveCommand<Unit, Task> StartVideoRecordingCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopRecordingCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowScreenshotViewerCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TakeScreenshotCommand { get; private set; }

    // View
    public ReactiveCommand<BorderSize, Unit> ChangeBorderSizeCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleFullScreenCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowTrainersCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowPrintOutputCommand { get; private set; }

    // Tape
    public ReactiveCommand<TapeSpeed, Unit> SetTapeLoadSpeedCommand { get; private set; }

    // Help
    public ReactiveCommand<Unit, Task> ShowAboutViewCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowKeyboardHelpViewCommand { get; private set; }

    // Interactions
    public Interaction<Unit, Unit?> ShowAboutView { get; }
    public Interaction<DebuggerViewModel, Unit?> ShowDebuggerView { get; }
    public Interaction<Unit, Unit?> ShowKeyboardHelpView { get; }
    public Interaction<PreferencesViewModel, Preferences?> ShowPreferencesView { get; }
    public Interaction<SelectFileViewModel, ArchiveEntry?> ShowSelectFileView { get; }
    public Interaction<TimeMachineViewModel, TimeMachineEntry?> ShowTimeMachineView { get; }
    public Interaction<ScreenshotViewModel, Unit?> ShowScreenshotView { get; }
    public Interaction<TrainerViewModel, Unit?> ShowTrainersView { get; }
    public Interaction<PrintOutputViewModel, Unit?> ShowPrintOutputView { get; }

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

        _statusBarTimer = new Timer(TimeSpan.FromSeconds(1));
        _statusBarTimer.AutoReset = true;
        _statusBarTimer.Elapsed += StatusBarTimerOnElapsed;

        var timeMachineEnabled = this.WhenAnyValue(x => x.IsTimeMachineEnabled);

        this.WhenAny(x => x.TapeLoadSpeed, x => x.Value)
            .Subscribe(_ => Emulator?.SetTapeLoadingSpeed(TapeLoadSpeed));

        this.WhenAny(x => x.IsTimeMachineEnabled, x => x.Value)
            .Subscribe(x => _timeMachine.IsEnabled = x);

        this.WhenAny(x => x.RecordingStatus, x => x.Value)
            .Subscribe(status => StatusBarViewModel.RecordingStatus = status);

        this.WhenAny(x => x.ComputerType, x => x.Value)
            .Subscribe(computerType => StatusBarViewModel.ComputerType = computerType);

        this.WhenAny(x => x.JoystickType, x => x.Value)
            .Subscribe(joystickType => StatusBarViewModel.JoystickType = joystickType);

        this.WhenAny(x => x.MouseType, x => x.Value)
            .Subscribe(mouseType => StatusBarViewModel.IsMouseEnabled = mouseType != MouseType.None);

        WindowOpenedCommand = ReactiveCommand.CreateFromTask(WindowOpenedAsync);
        WindowClosingCommand = ReactiveCommand.CreateFromTask<WindowClosingEventArgs>(WindowClosingAsync);
        KeyDownCommand = ReactiveCommand.Create<KeyEventArgs>(HandleKeyDown);
        TimeMachineResumeEmulatorCommand = ReactiveCommand.Create(HandleTimeMachineResumeEmulator);

        // File
        LoadFileCommand = ReactiveCommand.Create(HandleLoadFileAsync);
        SaveFileCommand = ReactiveCommand.Create(HandleSaveFileAsync);
        QuickSaveCommand = ReactiveCommand.Create(HandleQuickSave);
        QuickLoadCommand = ReactiveCommand.Create(HandleQuickLoad);
        ShowPreferencesViewCommand = ReactiveCommand.Create(OpenPreferencesWindow);
        ExitApplicationCommand = ReactiveCommand.Create(() => MainWindow?.Close());

        // Machine
        ChangeComputerType = ReactiveCommand.Create<ComputerType>(HandleChangeComputerType);
        ChangeRomCommand = ReactiveCommand.Create<RomType, Task>(HandleChangeRomAsync);
        ChangeJoystickType = ReactiveCommand.Create<JoystickType>(HandleChangeJoystickType);
        ChangeMouseType = ReactiveCommand.Create<MouseType>(HandleChangeMouseType);
        ToggleUlaPlus = ReactiveCommand.Create(HandleToggleUlaPlus);

        // Control
        SetEmulationSpeedCommand = ReactiveCommand.Create<string>(HandleSetEmulationSpeed);
        TogglePauseCommand = ReactiveCommand.Create(HandleTogglePause);
        ShowTimeMachineViewCommand = ReactiveCommand.Create(OpenTimeMachineWindow, timeMachineEnabled);
        ToggleMuteCommand = ReactiveCommand.Create(HandleToggleMute);
        TriggerNmiCommand = ReactiveCommand.Create(HandleTriggerNmi);
        ResetCommand = ReactiveCommand.Create(HandleMachineReset);
        HardResetCommand = ReactiveCommand.Create(HandleMachineHardReset);

        // Tools
        ShowDebuggerViewCommand = ReactiveCommand.Create(OpenDebuggerWindow);
        StartAudioRecordingCommand = ReactiveCommand.Create(HandleStartAudioRecordingAsync);
        StartVideoRecordingCommand = ReactiveCommand.Create(HandleStartVideoRecordingAsync);
        StopRecordingCommand = ReactiveCommand.Create(HandleStopRecording);
        ShowScreenshotViewerCommand = ReactiveCommand.Create(OpenScreenshotViewer);
        TakeScreenshotCommand = ReactiveCommand.Create(HandleTakeScreenshot);

        // View
        ToggleFullScreenCommand = ReactiveCommand.Create(HandleToggleFullScreen);
        ChangeBorderSizeCommand = ReactiveCommand.Create<BorderSize>(HandleChangeBorderSize);
        ShowTrainersCommand = ReactiveCommand.Create(OpenTrainersWindow);
        ShowPrintOutputCommand = ReactiveCommand.Create(OpenPrintOutputViewer);

        // Tape
        SetTapeLoadSpeedCommand = ReactiveCommand.Create<TapeSpeed>(HandleSetTapeLoadingSpeed);

        // Help
        ShowAboutViewCommand = ReactiveCommand.Create(OpenAboutWindow);
        ShowKeyboardHelpViewCommand = ReactiveCommand.Create(ShowKeyboardHelpWindow);

        // Interactions
        ShowAboutView = new Interaction<Unit, Unit?>();
        ShowDebuggerView = new Interaction<DebuggerViewModel, Unit?>();
        ShowKeyboardHelpView = new Interaction<Unit, Unit?>();
        ShowPreferencesView = new Interaction<PreferencesViewModel, Preferences?>();
        ShowSelectFileView = new Interaction<SelectFileViewModel, ArchiveEntry?>();
        ShowTimeMachineView = new Interaction<TimeMachineViewModel, TimeMachineEntry?>();
        ShowScreenshotView = new Interaction<ScreenshotViewModel, Unit?>();
        ShowTrainersView = new Interaction<TrainerViewModel, Unit?>();
        ShowPrintOutputView = new Interaction<PrintOutputViewModel, Unit?>();

        SpectrumScreen = _frameBufferConverter.ScreenBitmap;

        tapeManager.TapeStateChanged += HandleTapeStateChanged;

        _keyboardHook = new KeyboardHook();
        _keyboardHook.SpectrumKeyPressed  += HandleSpectrumKeyPressed;
        _keyboardHook.SpectrumKeyReleased += HandleSpectrumKeyReleased;
        _keyboardHook.Run();
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

    private async Task OpenAboutWindow() => await ShowAboutView.Handle(Unit.Default);

    private async Task OpenDebuggerWindow()
    {
        if (!IsPaused)
        {
            Pause(showOverlay: false);
        }

        _debuggerViewModel = new DebuggerViewModel(_debuggerContext, Emulator!, _preferences.Debugger);

        _debuggerViewModel.HardResetCommand.Subscribe(x => HandleMachineHardReset());
        _debuggerViewModel.ResetCommand.Subscribe(x => HandleMachineReset());

        this.WhenAny(x => x.IsPaused, x => x.Value)
            .Subscribe(isPaused => _debuggerViewModel?.HandlePause(isPaused));

        this.WhenAnyValue(x => x._debuggerViewModel)
            .Where(dvm => dvm != null)
            .Select(dvm => dvm!.WhenAnyValue(vm => vm.IsPaused))
            .Switch()
            .Subscribe(isPaused => IsPaused = isPaused);

        await ShowDebuggerView.Handle(_debuggerViewModel);
    }

    private async Task ShowKeyboardHelpWindow() => await ShowKeyboardHelpView.Handle(Unit.Default);

    public async Task OpenPreferencesWindow()
    {
        var resumeAfter = false;

        if (!IsPaused)
        {
            Pause();
            resumeAfter = true;
        }

        using var viewModel = new PreferencesViewModel(_preferences, _gamepadManager);
        var preferences = await ShowPreferencesView.Handle(viewModel);

        ThemeManager.SelectTheme(preferences?.Theme ?? _preferences.Theme);

        if (preferences != null)
        {
            _preferences = preferences;

            IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
            TapeLoadSpeed = preferences.Tape.LoadSpeed;

            IsTimeMachineEnabled = preferences.TimeMachine.IsEnabled;
            _timeMachine.SnapshotInterval = preferences.TimeMachine.SnapshotInterval;
            _timeMachine.MaxDuration = preferences.TimeMachine.MaxDuration;
            TimeMachineCountdownSeconds = preferences.TimeMachine.CountdownSeconds;

            HandleChangeJoystickType(preferences.Joystick.JoystickType);
            HandleChangeMouseType(preferences.Mouse.MouseType);

            Emulator.SetUlaPlus(IsUlaPlusEnabled);
            Emulator.SetTapeSettings(_preferences.Tape);

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

        var entry = await ShowTimeMachineView.Handle(viewModel);

        if (entry != null)
        {
            var snapshot = entry.GetSnapshot();

            if (snapshot == null)
            {
                return;
            }

            IsTimeMachineCountdownVisible = true;

            CreateEmulator(snapshot);
            Emulator?.Pause();
        }
        else
        {
            if (resumeAfter)
            {
                Resume();
            }
        }

        _isTimeMachineOpen = false;
    }

    private async Task OpenScreenshotViewer() =>
        await ShowScreenshotView.Handle(_screenshotViewModel);

    private async Task OpenTrainersWindow() =>
        await ShowTrainersView.Handle(new TrainerViewModel(Emulator!, _pokeFile));

    private async Task OpenPrintOutputViewer() =>
        await ShowPrintOutputView.Handle(new PrintOutputViewModel(Emulator!.Printer));

    private void StatusBarTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        var fps = _frameCount.ToString();

        Dispatcher.UIThread.Post(() => StatusBarViewModel.FramesPerSecond = fps);

        Interlocked.Exchange(ref _frameCount, 0);
    }

    private void EmulatorFrameCompleted(FrameBuffer frameBuffer, AudioBuffer audioBuffer)
    {
        // Keep max 50 FPS
        if (_renderStopwatch.Elapsed - _lastScreenRender < TimeSpan.FromMilliseconds(19))
        {
            return;
        }

        _lastScreenRender = _renderStopwatch.Elapsed;
        Interlocked.Increment(ref _frameCount);

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

        _preferences.Audio.IsMuted = IsMuted;

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
        _statusBarTimer.Start();
    }

    private void ConfigureEmulatorSettings()
    {
        Emulator.SetFloatingBusSupport(_preferences.IsFloatingBusEnabled);
        Emulator.SetAudioSettings(_preferences.Audio);
        Emulator.SetGamepad(_preferences.Joystick);
        Emulator.SetDivMMc(_preferences.DivMmc);
        Emulator.SetPrinter(_preferences.Printer);

        SetMouseCursor();

        StatusBarViewModel.IsDivMmcEnabled = _preferences.DivMmc.IsEnabled;
        StatusBarViewModel.IsMouseEnabled = Emulator!.MouseManager.MouseType != MouseType.None;
        StatusBarViewModel.IsPrinterEnabled = _preferences.Printer.IsZxPrinterEnabled;
        StatusBarViewModel.IsUlaPlusEnabled = IsUlaPlusEnabled;
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

    private void SetMouseCursor() =>
        MouseCursor = _preferences.Mouse is { MouseType: not MouseType.None, IsStandardMousePointerHidden: true }
            ? Cursor.Parse("None")
            : Cursor.Default;
}
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
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Emulation.Storage;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Emulation.Tape.Loader;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Recorder;
using OldBit.Spectron.Screen;
using OldBit.Spectron.Theming;
using ReactiveUI;
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

    private Emulator? Emulator { get; set; }
    private Preferences _preferences = new();
    private int _frameCount;
    private readonly Stopwatch _renderStopwatch = new();
    private TimeSpan _lastScreenRender = TimeSpan.Zero;
    private MediaRecorder? _mediaRecorder;
    private bool _canClose;
    private DebuggerViewModel? _debuggerViewModel;

    public Control ScreenControl { get; set; } = null!;
    public Window? MainWindow { get; set; }
    public WindowNotificationManager NotificationManager { get; set; } = null!;

    public StatusBarViewModel StatusBarViewModel { get; } = new();
    public TapeMenuViewModel TapeMenuViewModel { get; }
    public RecentFilesViewModel RecentFilesViewModel { get; }

    public ReactiveCommand<Unit, Unit> WindowOpenedCommand { get; private set; }
    public ReactiveCommand<WindowClosingEventArgs, Unit> WindowClosingCommand { get; private set; }

    public ReactiveCommand<KeyEventArgs, Unit> KeyDownCommand { get; private set; }
    public ReactiveCommand<KeyEventArgs, Unit> KeyUpCommand { get; private set; }

    public ReactiveCommand<Unit, Task> LoadFileCommand { get; private set; }
    public ReactiveCommand<Unit, Task> SaveFileCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> QuickSaveCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> QuickLoadCommand { get; private set; }
    public ReactiveCommand<Unit, Task> StartAudioRecordingCommand { get; private set; }
    public ReactiveCommand<Unit, Task> StartVideoRecordingCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopRecordingCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ExitApplicationCommand { get; private set; }

    public ReactiveCommand<BorderSize, Unit> ChangeBorderSizeCommand { get; private set; }
    public ReactiveCommand<RomType, Unit> ChangeRomCommand { get; private set; }
    public ReactiveCommand<ComputerType, Unit> ChangeComputerType { get; private set; }
    public ReactiveCommand<JoystickType, Unit> ChangeJoystickType { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleUlaPlus { get; private set; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> HardResetCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TogglePauseCommand { get; private set; }
    public ReactiveCommand<string, Unit> SetEmulationSpeedCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleFullScreenCommand { get; private set; }
    public ReactiveCommand<TapeSpeed, Unit> SetTapeLoadSpeedCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleMuteCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> TimeMachineResumeEmulatorCommand  { get; private set; }

    public ReactiveCommand<Unit, Task> ShowAboutViewCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowDebuggerViewCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowKeyboardHelpViewCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowPreferencesViewCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowTimeMachineViewCommand { get; private set; }

    public Interaction<Unit, Unit?> ShowAboutView { get; }
    public Interaction<DebuggerViewModel, Unit?> ShowDebuggerView { get; }
    public Interaction<Unit, Unit?> ShowKeyboardHelpView { get; }
    public Interaction<PreferencesViewModel, Preferences?> ShowPreferencesView { get; }
    public Interaction<SelectFileViewModel, ArchiveEntry?> ShowSelectFileView { get; }
    public Interaction<TimeMachineViewModel, TimeMachineEntry?> ShowTimeMachineView { get; }

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

        WindowOpenedCommand = ReactiveCommand.CreateFromTask(WindowOpenedAsync);
        WindowClosingCommand = ReactiveCommand.CreateFromTask<WindowClosingEventArgs>(WindowClosingAsync);
        KeyDownCommand = ReactiveCommand.Create<KeyEventArgs>(HandleKeyDown);
        KeyUpCommand = ReactiveCommand.Create<KeyEventArgs>(HandleKeyUp);

        LoadFileCommand = ReactiveCommand.Create(HandleLoadFileAsync);
        SaveFileCommand = ReactiveCommand.Create(HandleSaveFileAsync);
        QuickSaveCommand = ReactiveCommand.Create(HandleQuickSave);
        QuickLoadCommand = ReactiveCommand.Create(HandleQuickLoad);
        StartAudioRecordingCommand = ReactiveCommand.Create(HandleStartAudioRecordingAsync);
        StartVideoRecordingCommand = ReactiveCommand.Create(HandleStartVideoRecordingAsync);
        StopRecordingCommand = ReactiveCommand.Create(HandleStopRecording);
        ExitApplicationCommand = ReactiveCommand.Create(() => MainWindow?.Close());

        ChangeBorderSizeCommand = ReactiveCommand.Create<BorderSize>(HandleChangeBorderSize);
        ChangeRomCommand = ReactiveCommand.Create<RomType>(HandleChangeRom);
        ChangeComputerType = ReactiveCommand.Create<ComputerType>(HandleChangeComputerType);
        ChangeJoystickType = ReactiveCommand.Create<JoystickType>(HandleChangeJoystickType);
        ToggleUlaPlus = ReactiveCommand.Create(HandleToggleUlaPlus);
        TogglePauseCommand = ReactiveCommand.Create(HandleTogglePause);
        ResetCommand = ReactiveCommand.Create(HandleMachineReset);
        HardResetCommand = ReactiveCommand.Create(HandleMachineHardReset);
        SetEmulationSpeedCommand = ReactiveCommand.Create<string>(HandleSetEmulationSpeed);
        ToggleFullScreenCommand = ReactiveCommand.Create(HandleToggleFullScreen);
        SetTapeLoadSpeedCommand = ReactiveCommand.Create<TapeSpeed>(HandleSetTapeLoadingSpeed);
        ToggleMuteCommand = ReactiveCommand.Create(HandleToggleMute);
        TimeMachineResumeEmulatorCommand = ReactiveCommand.Create(HandleTimeMachineResumeEmulator);

        ShowAboutViewCommand = ReactiveCommand.Create(OpenAboutWindow);
        ShowDebuggerViewCommand = ReactiveCommand.Create(OpenDebuggerWindow);
        ShowKeyboardHelpViewCommand = ReactiveCommand.Create(ShowKeyboardHelpWindow);
        ShowPreferencesViewCommand = ReactiveCommand.Create(OpenPreferencesWindow);
        ShowTimeMachineViewCommand = ReactiveCommand.Create(OpenTimeMachineWindow, timeMachineEnabled);

        ShowAboutView = new Interaction<Unit, Unit?>();
        ShowDebuggerView = new Interaction<DebuggerViewModel, Unit?>();
        ShowKeyboardHelpView = new Interaction<Unit, Unit?>();
        ShowPreferencesView = new Interaction<PreferencesViewModel, Preferences?>();
        ShowSelectFileView = new Interaction<SelectFileViewModel, ArchiveEntry?>();
        ShowTimeMachineView = new Interaction<TimeMachineViewModel, TimeMachineEntry?>();

        SpectrumScreen = _frameBufferConverter.ScreenBitmap;

        tapeManager.TapeStateChanged += HandleTapeStateChanged;
    }

    public void OnViewClosed(object? viewModel)
    {
        if (viewModel is DebuggerViewModel)
        {
            Resume();
            _debuggerViewModel = null;
        }
    }

    private async Task OpenAboutWindow() => await ShowAboutView.Handle(Unit.Default);

    private async Task OpenDebuggerWindow()
    {
        if (!IsPaused)
        {
            Pause(showOverlay: false);
        }

        _debuggerViewModel = new DebuggerViewModel(_debuggerContext, Emulator!);

        this.WhenAny(x => x.IsPaused, x => x.Value)
            .Subscribe(isPaused => _debuggerViewModel?.HandlePause(isPaused));

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
            TapeLoadSpeed = preferences.TapeSettings.LoadSpeed;

            IsTimeMachineEnabled = preferences.TimeMachine.IsEnabled;
            _timeMachine.SnapshotInterval = preferences.TimeMachine.SnapshotInterval;
            _timeMachine.MaxDuration = preferences.TimeMachine.MaxDuration;
            TimeMachineCountdownSeconds = preferences.TimeMachine.CountdownSeconds;

            Emulator?.SetUlaPlus(IsUlaPlusEnabled);
            Emulator?.SetFloatingBusSupport(preferences.IsFloatingBusEnabled);
            Emulator?.SetAudioSettings(preferences.AudioSettings);
            Emulator?.SetTapeSettings(preferences.TapeSettings);
            Emulator?.SetGamepad(preferences.Joystick);
        }

        if (resumeAfter)
        {
            Resume();
        }
    }

    private async Task OpenTimeMachineWindow()
    {
        var resumeAfter = false;

        if (!IsPaused)
        {
            Pause();
            resumeAfter = true;
        }

        var viewModel = new TimeMachineViewModel(_timeMachine, _logger);

        var entry = await ShowTimeMachineView.Handle(viewModel);

        if (entry != null)
        {
            IsTimeMachineCountdownVisible = true;

            CreateEmulator(entry.Snapshot);
            Emulator?.Pause();
        }
        else
        {
            if (resumeAfter)
            {
                Resume();
            }
        }
    }

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

        IsMuted = _preferences.AudioSettings.IsMuted;

        IsTimeMachineEnabled = _preferences.TimeMachine.IsEnabled;
        _timeMachine.SnapshotInterval = _preferences.TimeMachine.SnapshotInterval;
        _timeMachine.MaxDuration = _preferences.TimeMachine.MaxDuration;
        TimeMachineCountdownSeconds = _preferences.TimeMachine.CountdownSeconds;

        await RecentFilesViewModel.LoadAsync();

        HandleChangeBorderSize(_preferences.BorderSize);

        if (_preferences.ResumeSettings.IsResumeEnabled)
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

        TapeLoadSpeed = _preferences.TapeSettings.LoadSpeed;
        Emulator?.SetTapeSettings(_preferences.TapeSettings);
    }

    private async Task WindowClosingAsync(WindowClosingEventArgs args)
    {
        if (_canClose)
        {
            return;
        }

        args.Cancel = true;

        Emulator?.Shutdown();

        _preferences.AudioSettings.IsMuted = IsMuted;

        await Task.WhenAll(
            _preferencesService.SaveAsync(_preferences),
            RecentFilesViewModel.SaveAsync(),
            _sessionService.SaveAsync(Emulator, _preferences.ResumeSettings));

        _canClose = true;
        MainWindow?.Close();
    }

    private void CreateEmulator(ComputerType computerType, RomType romType)
    {
        var emulator = _emulatorFactory.Create(computerType, romType);

        emulator.SetUlaPlus(_preferences.IsUlaPlusEnabled);
        emulator.JoystickManager.SetupJoystick(_preferences.Joystick.JoystickType);

        InitializeEmulator(emulator);
    }

    private void CreateEmulator(StateSnapshot stateSnapshot)
    {
        Emulator?.Reset();

        var emulator = _stateManager.CreateEmulator(stateSnapshot);

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
                _preferences.TapeSettings.IsAutoPlayEnabled && TapeLoadSpeed != TapeSpeed.Instant);
            emulator.SetUlaPlus(_preferences.IsUlaPlusEnabled);
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
        IsUlaPlusEnabled = Emulator.IsUlaPlusEnabled;

        Emulator.TapeLoadSpeed = TapeLoadSpeed;
        Emulator.FrameCompleted += EmulatorFrameCompleted;

        Emulator.SetFloatingBusSupport(_preferences.IsFloatingBusEnabled);
        Emulator.SetAudioSettings(_preferences.AudioSettings);
        Emulator.SetGamepad(_preferences.Joystick);

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

    private void CommandManagerOnCommandReceived(object? sender, CommandEventArgs e)
    {
        if (e.Command is GamepadActionCommand gamepadCommand)
        {
            if (gamepadCommand.State == InputState.Pressed)
            {
                return;
            }

            switch (gamepadCommand.Action)
            {
                case GamepadAction.Pause:
                    HandleTogglePause();
                    break;

                case GamepadAction.TimeTravel:
                    //HandleTimeTravel();
                    break;
            }
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
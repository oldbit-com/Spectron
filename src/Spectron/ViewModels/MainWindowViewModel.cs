using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Debugger;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.Storage;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Emulation.Tape.Loader;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Helpers;
using OldBit.Spectron.Models;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.ViewModels.Debugger;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace OldBit.Spectron.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const string DefaultTitle = "Spectron - ZX Spectrum Emulator";

    private readonly EmulatorFactory _emulatorFactory;
    private readonly TimeMachine _timeMachine;
    private readonly GamepadManager _gamepadManager;

    private readonly SnapshotLoader _snapshotLoader;
    private readonly Loader _loader;

    private readonly PreferencesService _preferencesService;
    private readonly SessionService _sessionService;
    private readonly DebuggerContext _debuggerContext;
    private readonly FrameBufferConverter _frameBufferConverter = new(4, 4);
    private readonly Timer _statusBarTimer;

    private Preferences _preferences = new();
    private int _frameCount;
    private readonly Stopwatch _renderStopwatch = new();
    private TimeSpan _lastScreenRender = TimeSpan.Zero;

    public Emulator? Emulator { get; private set; }
    public Control ScreenControl { get; set; } = null!;
    public Window? MainWindow { get; set; }
    public StatusBarViewModel StatusBar { get; } = new();
    public TapeMenuViewModel TapeMenuViewModel { get; }
    public RecentFilesViewModel RecentFilesViewModel { get; }

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
        SnapshotLoader snapshotLoader,
        Loader loader,
        PreferencesService preferencesService,
        SessionService sessionService,
        RecentFilesViewModel recentFilesViewModel,
        TapeMenuViewModel tapeMenuViewModel,
        DebuggerContext debuggerContext)
    {
        _emulatorFactory = emulatorFactory;
        _timeMachine = timeMachine;
        _gamepadManager = gamepadManager;
        _snapshotLoader = snapshotLoader;
        _loader = loader;
        _preferencesService = preferencesService;
        _sessionService = sessionService;
        _debuggerContext = debuggerContext;
        RecentFilesViewModel = recentFilesViewModel;
        TapeMenuViewModel = tapeMenuViewModel;
        recentFilesViewModel.OpenRecentFileAsync = async fileName => await HandleLoadFileAsync(fileName);

        _statusBarTimer = new Timer(TimeSpan.FromSeconds(1));
        _statusBarTimer.AutoReset = true;
        _statusBarTimer.Elapsed += StatusBarTimerOnElapsed;

        var emulatorNotNull = this.WhenAnyValue(x => x.Emulator).Select(emulator => emulator is null);

        var timeMachineEnabled = this.WhenAnyValue(x => x.IsTimeMachineEnabled);

        this.WhenAny(x => x.WindowState, x => x.Value)
            .Subscribe(x => WindowStateCommandName = x == WindowState.FullScreen ? "Exit Full Screen" : "Enter Full Screen");

        this.WhenAny(x => x.TapeLoadSpeed, x => x.Value)
            .Subscribe(_ => Emulator?.SetTapeLoadingSpeed(TapeLoadSpeed));

        this.WhenAny(x => x.IsTimeMachineEnabled, x => x.Value)
            .Subscribe(x => _timeMachine.IsEnabled = x);

        WindowOpenedCommand = ReactiveCommand.CreateFromTask(WindowOpenedAsync);
        WindowClosingCommand = ReactiveCommand.CreateFromTask(WindowClosingAsync);
        KeyDownCommand = ReactiveCommand.Create<KeyEventArgs>(HandleKeyDown);
        KeyUpCommand = ReactiveCommand.Create<KeyEventArgs>(HandleKeyUp);
        LoadFileCommand = ReactiveCommand.Create(HandleLoadFileAsync);
        SaveFileCommand = ReactiveCommand.Create(HandleSaveFileAsync, emulatorNotNull);
        ChangeBorderSizeCommand = ReactiveCommand.Create<BorderSize>(HandleChangeBorderSize);
        ChangeRomCommand = ReactiveCommand.Create<RomType>(HandleChangeRom);
        ChangeComputerType = ReactiveCommand.Create<ComputerType>(HandleChangeComputerType);
        ChangeJoystickType = ReactiveCommand.Create<JoystickType>(HandleChangeJoystickType);
        ToggleUlaPlus = ReactiveCommand.Create(HandleToggleUlaPlus);
        TogglePauseCommand = ReactiveCommand.Create(HandleTogglePause, emulatorNotNull);
        ResetCommand = ReactiveCommand.Create(HandleMachineReset, emulatorNotNull);
        HardResetCommand = ReactiveCommand.Create(HandleMachineHardReset, emulatorNotNull);
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

        SpectrumScreen = _frameBufferConverter.Bitmap;
    }

    private async Task OpenAboutWindow() => await ShowAboutView.Handle(Unit.Default);

    private async Task OpenDebuggerWindow()
    {
        if (!IsPaused)
        {
            HandleTogglePause();
        }

        await ShowDebuggerView.Handle(new DebuggerViewModel(this, _debuggerContext));
    }

    private async Task ShowKeyboardHelpWindow() => await ShowKeyboardHelpView.Handle(Unit.Default);

    public async Task OpenPreferencesWindow()
    {
        Emulator?.Pause();

        using var viewModel = new PreferencesViewModel(_preferences, _gamepadManager);
        var preferences = await ShowPreferencesView.Handle(viewModel);

        if (preferences != null)
        {
            _preferences = preferences;

            //TapeLoadingSpeed = preferences.TapeLoadingSpeed;

            IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;

            IsTimeMachineEnabled = preferences.TimeMachine.IsEnabled;
            _timeMachine.SnapshotInterval = preferences.TimeMachine.SnapshotInterval;
            _timeMachine.MaxDuration = preferences.TimeMachine.MaxDuration;

            Emulator?.SetUlaPlus(IsUlaPlusEnabled);
            Emulator?.SetAudioSettings(preferences.AudioSettings);
            Emulator?.SetTapeSavingSettings(preferences.TapeSaving);
            Emulator?.SetGamepad(preferences.Joystick);
        }

        Emulator?.Resume();
    }

    private async Task OpenTimeMachineWindow()
    {
        Emulator?.Pause();

        var viewModel = new TimeMachineViewModel(_timeMachine);

        var entry = await ShowTimeMachineView.Handle(viewModel);

        if (entry != null)
        {
            IsTimeMachineCountdownVisible = true;

            CreateEmulator(entry.Snapshot);
            Emulator?.Pause();
        }
        else
        {
            Emulator?.Resume();
            IsPaused = false;
        }
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
        // Keep max 50 FPS
        if (_renderStopwatch.Elapsed - _lastScreenRender < TimeSpan.FromMilliseconds(19))
        {
            return;
        }

        _lastScreenRender = _renderStopwatch.Elapsed;
        Interlocked.Increment(ref _frameCount);

        Dispatcher.UIThread.Post(() =>
        {
            _frameBufferConverter.UpdateBitmap(framebuffer);
            ScreenControl.InvalidateVisual();
        });
    }

    private async Task WindowOpenedAsync()
    {
        _preferences = await _preferencesService.LoadAsync();

        IsMuted = _preferences.AudioSettings.IsMuted;

        IsTimeMachineEnabled = _preferences.TimeMachine.IsEnabled;
        _timeMachine.SnapshotInterval = _preferences.TimeMachine.SnapshotInterval;
        _timeMachine.MaxDuration = _preferences.TimeMachine.MaxDuration;

        await RecentFilesViewModel.LoadAsync();

        HandleChangeBorderSize(_preferences.BorderSize);

        if (_preferences.ResumeSettings.IsResumeEnabled)
        {
            var snapshot = await _sessionService.LoadAsync();

            if (snapshot != null)
            {
                CreateEmulator(snapshot);
                SetTitle();
            }
        }

        if (Emulator == null)
        {
            CreateEmulator(_preferences.ComputerType, _preferences.RomType);
        }

        Emulator?.SetTapeSavingSettings(_preferences.TapeSaving);
    }

    private async Task WindowClosingAsync()
    {
        Emulator?.Shutdown();

        _preferences.AudioSettings.IsMuted = IsMuted;

        await Task.WhenAll(
            _preferencesService.SaveAsync(_preferences),
            RecentFilesViewModel.SaveAsync(),
            _sessionService.SaveAsync(Emulator, _preferences.ResumeSettings));
    }

    private void CreateEmulator(ComputerType computerType, RomType romType)
    {
        var emulator = _emulatorFactory.Create(computerType, romType);

        emulator.SetUlaPlus(_preferences.IsUlaPlusEnabled);
        emulator.JoystickManager.SetupJoystick(_preferences.Joystick.JoystickType);

        InitializeEmulator(emulator);
    }

    private void CreateEmulator(SzxFile snapshot)
    {
        var emulator = _snapshotLoader.Load(snapshot);

        InitializeEmulator(emulator);
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
        Emulator.RenderScreen += EmulatorOnRenderScreen;

        Emulator.SetAudioSettings(_preferences.AudioSettings);
        Emulator.SetGamepad(_preferences.Joystick);

        if (IsMuted)
        {
            Emulator.AudioManager.Mute();
        }

        _renderStopwatch.Restart();
        _lastScreenRender = TimeSpan.Zero;

        Emulator.CommandManager.CommandReceived += CommandManagerOnCommandReceived;

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
        Emulator.RenderScreen -= EmulatorOnRenderScreen;
        Emulator.CommandManager.CommandReceived -= CommandManagerOnCommandReceived;
        Emulator = null;
    }

    private void SetTitle()
    {
        if (RecentFilesViewModel.CurrentFileName == string.Empty)
        {
            Title = DefaultTitle;
            return;
        }

        Title = $"S{DefaultTitle} [{RecentFilesViewModel.CurrentFileName}]";
    }
}
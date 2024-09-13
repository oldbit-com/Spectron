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
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Snapshot;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Helpers;
using OldBit.Spectron.Models;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Views;
using OldBit.ZXTape.Szx;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace OldBit.Spectron.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly EmulatorFactory _emulatorFactory;
    private readonly TimeMachine _timeMachine;
    private readonly SnapshotFile _snapshotFile;
    private readonly SzxSnapshot _szxSnapshot;
    private readonly PreferencesService _preferencesService;
    private readonly SessionService _sessionService;
    private readonly FrameBufferConverter _frameBufferConverter = new(4, 4);
    private readonly Timer _statusBarTimer;

    private Emulator? Emulator { get; set; }
    private HelpKeyboardView? _helpKeyboardView;

    private bool _isResumeEnabled;
    private bool _useCorsorKeysAsJoystick;
    private int _frameCount;
    private readonly Stopwatch _renderStopwatch = new();
    private TimeSpan _lastScreenRender = TimeSpan.Zero;

    public Control ScreenControl { get; set; } = null!;
    public Window? MainWindow { get; set; }
    public StatusBarViewModel StatusBar { get; } = new();
    public TapeMenuViewModel TapeMenuViewModel { get; } = new();
    public TimeMachineViewModel TimeMachineViewModel { get; }
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
    public ReactiveCommand<Unit, Unit> TogglePauseCommand { get; private set; }
    public ReactiveCommand<string, Unit> SetEmulationSpeedCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleFullScreenCommand { get; private set; }
    public ReactiveCommand<TapeLoadingSpeed, Unit> SetTapeLoadSpeedCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> HelpKeyboardCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ShowTimeMachineCommand { get; private set; }

    public Interaction<PreferencesViewModel, Preferences?> ShowPreferencesView { get; }

    public MainWindowViewModel(
        EmulatorFactory emulatorFactory,
        TimeMachine timeMachine,
        SnapshotFile snapshotFile,
        SzxSnapshot szxSnapshot,
        PreferencesService preferencesService,
        SessionService sessionService,
        RecentFilesViewModel recentFilesViewModel,
        TimeMachineViewModel timeMachineViewModel)
    {
        _emulatorFactory = emulatorFactory;
        _timeMachine = timeMachine;
        _snapshotFile = snapshotFile;
        _szxSnapshot = szxSnapshot;
        _preferencesService = preferencesService;
        _sessionService = sessionService;
        RecentFilesViewModel = recentFilesViewModel;
        TimeMachineViewModel = timeMachineViewModel;
        recentFilesViewModel.OpenRecentFileAsync = async fileName => await HandleLoadFileAsync(fileName);

        _statusBarTimer = new Timer(TimeSpan.FromSeconds(1));
        _statusBarTimer.AutoReset = true;
        _statusBarTimer.Elapsed += StatusBarTimerOnElapsed;

        var emulatorNotNull = this.WhenAnyValue(x => x.Emulator).Select(emulator => emulator is null);

        var timeMachineEnabled = this.WhenAnyValue(x => x._timeMachine.IsEnabled).Select(x => x);

        this.WhenAny(x => x.WindowState, x => x.Value)
            .Subscribe(x => WindowStateCommandName = x == WindowState.FullScreen ? "Exit Full Screen" : "Enter Full Screen");

        this.WhenAny(x => x.TapeLoadingSpeed, x => x.Value)
            .Subscribe(_ => Emulator?.SetTapeLoadingSpeed(TapeLoadingSpeed));

        timeMachineEnabled.Subscribe(Console.WriteLine);

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
        SetEmulationSpeedCommand = ReactiveCommand.Create<string>(HandleSetEmulationSpeed);
        ToggleFullScreenCommand = ReactiveCommand.Create(HandleToggleFullScreen);
        SetTapeLoadSpeedCommand = ReactiveCommand.Create<TapeLoadingSpeed>(HandleSetTapeLoadingSpeed);
        HelpKeyboardCommand = ReactiveCommand.Create(HandleHelpKeyboardCommand);
        ShowTimeMachineCommand = ReactiveCommand.Create(HandleShowTimeMachineCommand, timeMachineEnabled);

        ShowPreferencesView = new Interaction<PreferencesViewModel, Preferences?>();

        TimeMachineViewModel.OnTimeTravel = HandleTimeTravel;
        SpectrumScreen = _frameBufferConverter.Bitmap;
    }

    public async Task OpenPreferencesWindow()
    {
        Emulator?.Pause();

        var viewModel = new PreferencesViewModel(Preferences);
        var preferences = await ShowPreferencesView.Handle(viewModel);

        if (preferences != null)
        {
            var initialize = ComputerType != preferences.ComputerType || RomType != preferences.RomType;

            ComputerType = preferences.ComputerType;
            IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
            RomType = preferences.RomType;

            JoystickType = preferences.Joystick.JoystickType;
            Emulator?.JoystickManager.SetupJoystick(JoystickType);
            // TapeLoadingSpeed = preferences.TapeLoadingSpeed;

            _isResumeEnabled = preferences.IsResumeEnabled;
            _useCorsorKeysAsJoystick = preferences.Joystick.UseCursorKeys;

            _timeMachine.IsEnabled = preferences.TimeMachine.IsEnabled;
            _timeMachine.SnapshotInterval = preferences.TimeMachine.SnapshotInterval;
            _timeMachine.MaxDuration = preferences.TimeMachine.MaxDuration;

            if (initialize)
            {
                CreateEmulator();
            }
        }

        Emulator?.Resume();
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
        var preferences = await _preferencesService.LoadAsync();

        HandleChangeBorderSize(preferences.BorderSize);
        ComputerType = preferences.ComputerType;
        IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
        RomType = preferences.RomType == RomType.Custom ? RomType.Original : preferences.RomType;
        JoystickType = preferences.Joystick.JoystickType;
        TapeLoadingSpeed = preferences.TapeLoadingSpeed;

        _isResumeEnabled = preferences.IsResumeEnabled;
        _useCorsorKeysAsJoystick = preferences.Joystick.UseCursorKeys;

        _timeMachine.IsEnabled = preferences.TimeMachine.IsEnabled;
        _timeMachine.SnapshotInterval = preferences.TimeMachine.SnapshotInterval;
        _timeMachine.MaxDuration = preferences.TimeMachine.MaxDuration;

        await RecentFilesViewModel.LoadAsync();
        var snapshot = await _sessionService.LoadAsync();

        CreateEmulator(snapshot);
    }

    private async Task WindowClosingAsync() => await Task.WhenAll(
        _preferencesService.SaveAsync(Preferences),
        RecentFilesViewModel.SaveAsync(),
        _sessionService.SaveAsync(Emulator, _isResumeEnabled));

    private void CreateEmulator(SzxFile? snapshot = null)
    {
        ShutdownEmulator();

        var emulator = snapshot == null ?
            _emulatorFactory.Create(ComputerType, RomType) :
            _szxSnapshot.CreateEmulator(snapshot);

        InitializeEmulator(emulator);
    }

    private void InitializeEmulator(Emulator emulator)
    {
        Emulator?.Stop();
        Emulator = emulator;
        IsPaused = false;

        ComputerType = emulator.ComputerType;
        RomType = emulator.RomType;
        JoystickType = emulator.JoystickManager.JoystickType;
        IsUlaPlusEnabled = emulator.IsUlaPlusEnabled;

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

    private void ShutdownEmulator()
    {
        if (Emulator == null)
        {
            return;
        }

        Emulator.Stop();
        Emulator.RenderScreen -= EmulatorOnRenderScreen;
        Emulator = null;
    }

    private Preferences Preferences => new()
    {
        BorderSize = BorderSize,
        ComputerType = ComputerType,
        IsUlaPlusEnabled = IsUlaPlusEnabled,
        RomType = RomType,
        Joystick = new JoystickSettings
        {
            JoystickType = JoystickType,
            UseCursorKeys = _useCorsorKeysAsJoystick
        },
        TapeLoadingSpeed = TapeLoadingSpeed,
        IsResumeEnabled = _isResumeEnabled,

        TimeMachine = new TimeMachineSettings
        {
            IsEnabled = _timeMachine.IsEnabled,
            SnapshotInterval = _timeMachine.SnapshotInterval,
            MaxDuration = _timeMachine.MaxDuration
        }
    };
}
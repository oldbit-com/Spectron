﻿using System;
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
using OldBit.Spectron.Emulation.Tape.Loader;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Helpers;
using OldBit.Spectron.Models;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Views;
using OldBit.ZX.Files.Szx;
using ReactiveUI;
using Timer = System.Timers.Timer;

namespace OldBit.Spectron.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly EmulatorFactory _emulatorFactory;
    private readonly TimeMachine _timeMachine;

    private readonly SnapshotLoader _snapshotLoader;
    private readonly Loader _loader;

    private readonly PreferencesService _preferencesService;
    private readonly SessionService _sessionService;
    private readonly FrameBufferConverter _frameBufferConverter = new(4, 4);
    private readonly Timer _statusBarTimer;

    private Emulator? Emulator { get; set; }
    private HelpKeyboardView? _helpKeyboardView;

    private Preferences _preferences = new();
    private bool _isResumeEnabled;
    private bool _useCursorKeysAsJoystick;
    private int _frameCount;
    private readonly Stopwatch _renderStopwatch = new();
    private TimeSpan _lastScreenRender = TimeSpan.Zero;

    public Control ScreenControl { get; set; } = null!;
    public Window? MainWindow { get; set; }
    public StatusBarViewModel StatusBar { get; } = new();
    public TapeMenuViewModel TapeMenuViewModel { get; }
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
    public ReactiveCommand<TapeSpeed, Unit> SetTapeLoadSpeedCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> HelpKeyboardCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowPreferencesViewCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ShowAboutViewCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ShowTimeMachineCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> ToggleMuteCommand { get; private set; }

    public Interaction<PreferencesViewModel, Preferences?> ShowPreferencesView { get; }
    public Interaction<Unit, Unit?> ShowAboutView { get; }

    public MainWindowViewModel(
        EmulatorFactory emulatorFactory,
        TimeMachine timeMachine,
        SnapshotLoader snapshotLoader,
        Loader loader,
        PreferencesService preferencesService,
        SessionService sessionService,
        RecentFilesViewModel recentFilesViewModel,
        TimeMachineViewModel timeMachineViewModel,
        TapeMenuViewModel tapeMenuViewModel)
    {
        _emulatorFactory = emulatorFactory;
        _timeMachine = timeMachine;
        _snapshotLoader = snapshotLoader;
        _loader = loader;
        _preferencesService = preferencesService;
        _sessionService = sessionService;
        RecentFilesViewModel = recentFilesViewModel;
        TimeMachineViewModel = timeMachineViewModel;
        TapeMenuViewModel = tapeMenuViewModel;
        recentFilesViewModel.OpenRecentFileAsync = async fileName => await HandleLoadFileAsync(fileName);

        _statusBarTimer = new Timer(TimeSpan.FromSeconds(1));
        _statusBarTimer.AutoReset = true;
        _statusBarTimer.Elapsed += StatusBarTimerOnElapsed;

        var emulatorNotNull = this.WhenAnyValue(x => x.Emulator).Select(emulator => emulator is null);

        var timeMachineEnabled = this.WhenAnyValue(x => x._timeMachine.IsEnabled).Select(x => x);

        this.WhenAny(x => x.WindowState, x => x.Value)
            .Subscribe(x => WindowStateCommandName = x == WindowState.FullScreen ? "Exit Full Screen" : "Enter Full Screen");

        this.WhenAny(x => x.TapeLoadSpeed, x => x.Value)
            .Subscribe(_ => Emulator?.SetTapeLoadingSpeed(TapeLoadSpeed));

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
        SetTapeLoadSpeedCommand = ReactiveCommand.Create<TapeSpeed>(HandleSetTapeLoadingSpeed);
        HelpKeyboardCommand = ReactiveCommand.Create(HandleHelpKeyboardCommand);
        ShowPreferencesViewCommand = ReactiveCommand.Create(OpenPreferencesWindow);
        ShowAboutViewCommand = ReactiveCommand.Create(OpenAboutView);
        ShowTimeMachineCommand = ReactiveCommand.Create(HandleShowTimeMachineCommand, timeMachineEnabled);
        ToggleMuteCommand = ReactiveCommand.Create(HandleToggleMute);

        ShowPreferencesView = new Interaction<PreferencesViewModel, Preferences?>();
        ShowAboutView = new Interaction<Unit, Unit?>();

        TimeMachineViewModel.OnTimeTravel = HandleTimeTravel;
        SpectrumScreen = _frameBufferConverter.Bitmap;
    }

    private async Task OpenAboutView()
    {
        await ShowAboutView.Handle(Unit.Default);
    }

    public async Task OpenPreferencesWindow()
    {
        Emulator?.Pause();

        var viewModel = new PreferencesViewModel(Preferences);
        var preferences = await ShowPreferencesView.Handle(viewModel);

        if (preferences != null)
        {
            _preferences = preferences;

            var initialize = ComputerType != preferences.ComputerType || RomType != preferences.RomType;

            ComputerType = preferences.ComputerType;
            IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
            RomType = preferences.RomType;

            JoystickType = preferences.Joystick.JoystickType;
            Emulator?.JoystickManager.SetupJoystick(JoystickType);
            // TapeLoadingSpeed = preferences.TapeLoadingSpeed;

            _isResumeEnabled = preferences.IsResumeEnabled;
            _useCursorKeysAsJoystick = preferences.Joystick.UseCursorKeys;

            _timeMachine.IsEnabled = preferences.TimeMachine.IsEnabled;
            _timeMachine.SnapshotInterval = preferences.TimeMachine.SnapshotInterval;
            _timeMachine.MaxDuration = preferences.TimeMachine.MaxDuration;

            if (initialize)
            {
                CreateEmulator();
            }

            Emulator?.SetAudioSettings(preferences.AudioSettings);
            Emulator?.SetTapeSavingSettings(preferences.TapeSaving);
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

        HandleChangeBorderSize(_preferences.BorderSize);

        ComputerType = _preferences.ComputerType;
        IsUlaPlusEnabled = _preferences.IsUlaPlusEnabled;
        RomType = _preferences.RomType == RomType.Custom ? RomType.Original : _preferences.RomType;
        JoystickType = _preferences.Joystick.JoystickType;
        TapeLoadSpeed = _preferences.TapeLoadSpeed;

        _isResumeEnabled = _preferences.IsResumeEnabled;
        _useCursorKeysAsJoystick = _preferences.Joystick.UseCursorKeys;

        _timeMachine.IsEnabled = _preferences.TimeMachine.IsEnabled;
        _timeMachine.SnapshotInterval = _preferences.TimeMachine.SnapshotInterval;
        _timeMachine.MaxDuration = _preferences.TimeMachine.MaxDuration;

        await RecentFilesViewModel.LoadAsync();
        var snapshot = await _sessionService.LoadAsync();

        CreateEmulator(snapshot);

        Emulator?.SetTapeSavingSettings(_preferences.TapeSaving);
        Emulator?.SetAudioSettings(_preferences.AudioSettings);
    }

    private async Task WindowClosingAsync()
    {
        Emulator?.Shutdown();

        await Task.WhenAll(
            _preferencesService.SaveAsync(Preferences),
            RecentFilesViewModel.SaveAsync(),
            _sessionService.SaveAsync(Emulator, _isResumeEnabled));
    }

    private void CreateEmulator(SzxFile? snapshot = null)
    {
        ShutdownEmulator();

        var emulator = snapshot == null ?
            _emulatorFactory.Create(ComputerType, RomType) :
            _snapshotLoader.Load(snapshot);

        InitializeEmulator(emulator);
    }

    private void InitializeEmulator(Emulator emulator)
    {
        ShutdownEmulator();

        Emulator = emulator;
        IsPaused = false;

        ComputerType = emulator.ComputerType;
        RomType = emulator.RomType;
        JoystickType = emulator.JoystickManager.JoystickType;
        IsUlaPlusEnabled = emulator.IsUlaPlusEnabled;

        Emulator.IsUlaPlusEnabled = IsUlaPlusEnabled;
        Emulator.TapeLoadSpeed = TapeLoadSpeed;
        Emulator.JoystickManager.SetupJoystick(JoystickType);
        Emulator.RenderScreen += EmulatorOnRenderScreen;

        Emulator.AudioManager.IsBeeperEnabled = _preferences.AudioSettings.IsBeeperEnabled;
        Emulator.AudioManager.IsAyEnabled = _preferences.AudioSettings.IsAyAudioEnabled;
        Emulator.AudioManager.IsAySupportedStandardSpectrum = _preferences.AudioSettings.IsAySupportedStandardSpectrum;

        if (IsMuted)
        {
            Emulator.AudioManager.Mute();
        }

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

        Emulator.Shutdown();
        Emulator.RenderScreen -= EmulatorOnRenderScreen;
        Emulator = null;
    }

    private Preferences Preferences => new()
    {
        BorderSize = BorderSize,
        ComputerType = ComputerType,
        IsUlaPlusEnabled = IsUlaPlusEnabled,
        RomType = RomType,
        AudioSettings = new AudioSettings
        {
            IsBeeperEnabled = Emulator?.AudioManager.IsBeeperEnabled ?? true,
            IsAyAudioEnabled = Emulator?.AudioManager.IsAyEnabled ?? true,
            IsAySupportedStandardSpectrum = Emulator?.AudioManager.IsAySupportedStandardSpectrum ?? true
        },
        Joystick = new JoystickSettings
        {
            JoystickType = JoystickType,
            UseCursorKeys = _useCursorKeysAsJoystick
        },
        TapeLoadSpeed = TapeLoadSpeed,
        IsResumeEnabled = _isResumeEnabled,

        TimeMachine = new TimeMachineSettings
        {
            IsEnabled = _timeMachine.IsEnabled,
            SnapshotInterval = _timeMachine.SnapshotInterval,
            MaxDuration = _timeMachine.MaxDuration
        },

        TapeSaving = new TapeSavingSettings(
            Emulator?.TapeManager.IsTapeSaveEnabled ?? true,
            Emulator?.TapeManager.TapeSaveSpeed ?? TapeSpeed.Instant)
    };
}
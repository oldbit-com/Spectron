using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Threading;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Theming;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class PreferencesViewModel : ReactiveObject, IDisposable
{
    private readonly GamepadManager _gamepadManager;
    private readonly GamepadSettings _gamepadSettings;

    public ReactiveCommand<Unit, Preferences> UpdatePreferencesCommand { get; }

    public Interaction<GamepadMappingViewModel, List<GamepadMapping>?> ShowGamepadMappingView { get; }

    public GamepadMappingViewModel GamepadMappingViewModel { get; }

    public PreferencesViewModel(Preferences preferences, GamepadManager gamepadManager)
    {
        _gamepadManager = gamepadManager;
        _gamepadSettings = preferences.Joystick.GamepadSettings;

        GamepadControllers = new ObservableCollection<GamepadController>(_gamepadManager.Controllers);
        GamepadMappingViewModel = new GamepadMappingViewModel(_gamepadManager);

        _gamepadManager.ControllerChanged += GamepadManagerOnControllerChanged;

        this.WhenAnyValue(x => x.GamepadControllerId)
            .Buffer(2, 1)
            .Select(b => (Previous: b[0], Current: b[1]))
            .Subscribe(value =>
            {
                if (value.Previous != Guid.Empty)
                {
                    _gamepadSettings.Mappings[value.Previous] = GamepadMappingViewModel.GetConfiguredMappings();
                }

                GamepadMappingViewModel.UpdateView(value.Current, _gamepadSettings);
            });

        this.WhenAnyValue(x => x.Theme).Subscribe(ThemeManager.SelectTheme);

        Theme = preferences.Theme;

        ComputerType = preferences.ComputerType;
        IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
        RomType = preferences.RomType;

        JoystickType = preferences.Joystick.JoystickType;
        EmulateUsingKeyboard = preferences.Joystick.EmulateUsingKeyboard;
        GamepadControllerId = _gamepadManager.Controllers.FirstOrDefault(
            controller => controller.ControllerId == preferences.Joystick.GamepadControllerId)?.ControllerId ?? GamepadController.None.ControllerId;

        IsResumeEnabled = preferences.ResumeSettings.IsResumeEnabled;
        ShouldIncludeTapeInResume = preferences.ResumeSettings.ShouldIncludeTape;
        ShouldIncludeTimeMachineInResume = preferences.ResumeSettings.ShouldIncludeTimeMachine;

        IsBeeperEnabled = preferences.AudioSettings.IsBeeperEnabled;
        IsAyEnabled = preferences.AudioSettings.IsAyAudioEnabled;
        IsAySupportedStandardSpectrum = preferences.AudioSettings.IsAySupportedStandardSpectrum;
        StereoMode = preferences.AudioSettings.StereoMode;

        IsTimeMachineEnabled = preferences.TimeMachine.IsEnabled;
        SnapshotInterval = preferences.TimeMachine.SnapshotInterval.TotalSeconds;
        MaxDuration = preferences.TimeMachine.MaxDuration.TotalSeconds;

        IsTapeSaveEnabled = preferences.TapeSaving.IsEnabled;
        TapeSaveSpeed = preferences.TapeSaving.Speed;

        UpdatePreferencesCommand = ReactiveCommand.Create(UpdatePreferences);

        ShowGamepadMappingView = new Interaction<GamepadMappingViewModel, List<GamepadMapping>?>();
    }

    private void GamepadManagerOnControllerChanged(object? sender, ControllerChangedEventArgs e)
    {
        switch (e.Action)
        {
            case ControllerChangedAction.Added:
                Dispatcher.UIThread.Post(() =>
                {
                    GamepadControllers.Add(e.Controller);
                    GamepadControllerId = e.Controller.ControllerId;
                });

                break;

            case ControllerChangedAction.Removed:
                Dispatcher.UIThread.Post(() =>
                {
                    GamepadControllers.Remove(e.Controller);
                    GamepadControllerId = GamepadController.None.ControllerId;
                });

                break;
        }
    }

    private Preferences UpdatePreferences()
    {
        if (GamepadControllerId != GamepadController.None.ControllerId)
        {
            _gamepadSettings.Mappings[GamepadControllerId] = GamepadMappingViewModel.GetConfiguredMappings();
        }

        return new Preferences
        {
            Theme = Theme,
            ComputerType = ComputerType,
            IsUlaPlusEnabled = IsUlaPlusEnabled,
            RomType = RomType,
            Joystick = new JoystickSettings
            {
                JoystickType = JoystickType,
                EmulateUsingKeyboard = EmulateUsingKeyboard,
                GamepadControllerId = GamepadControllerId,
                GamepadSettings = _gamepadSettings,
            },

            ResumeSettings = new ResumeSettings
            {
                IsResumeEnabled = IsResumeEnabled,
                ShouldIncludeTape = ShouldIncludeTapeInResume,
                ShouldIncludeTimeMachine = ShouldIncludeTimeMachineInResume
            },

            AudioSettings = new AudioSettings
            {
                IsBeeperEnabled = IsBeeperEnabled,
                IsAyAudioEnabled = IsAyEnabled,
                IsAySupportedStandardSpectrum = IsAySupportedStandardSpectrum,
                StereoMode = StereoMode,
            },

            TimeMachine = new TimeMachineSettings
            {
                IsEnabled = IsTimeMachineEnabled,
                SnapshotInterval = TimeSpan.FromSeconds(SnapshotInterval),
                MaxDuration = TimeSpan.FromSeconds(MaxDuration)
            },

            TapeSaving = new TapeSavingSettings(IsTapeSaveEnabled, TapeSaveSpeed)
        };
    }

    public List<NameValuePair<TapeSpeed>> TapeSpeeds { get; } =
    [
        new("Normal", TapeSpeed.Normal),
        new("Accelerated", TapeSpeed.Accelerated),
        new("Instant", TapeSpeed.Instant)
    ];

    public List<NameValuePair<ComputerType>> ComputerTypes { get; } =
    [
        new("ZX Spectrum 16k", ComputerType.Spectrum16K),
        new("ZX Spectrum 48k", ComputerType.Spectrum48K),
        new("ZX Spectrum 128k", ComputerType.Spectrum128K),
    ];

    public List<NameValuePair<RomType>> RomTypes { get; } =
    [
        new("Original", RomType.Original),
        new("Gosh Wonderful", RomType.GoshWonderful),
        new("Busy Soft v1.40", RomType.BusySoft),
        new("J.G. Harston v0.77", RomType.Harston),
        new("Diagnostic: Retroleum v1.71", RomType.Retroleum),
        new("Diagnostic: B. Alford v1.37", RomType.BrendanAlford),
    ];

    public List<NameValuePair<JoystickType>> JoystickTypes { get; } =
    [
        new("None", JoystickType.None),
        new("Kempston", JoystickType.Kempston),
        new("Sinclair 1", JoystickType.Sinclair1),
        new("Sinclair 2", JoystickType.Sinclair2),
        new("Cursor", JoystickType.Cursor),
        new("Fuller", JoystickType.Fuller),
    ];

    public List<NameValuePair<StereoMode>> StereoModes { get; } =
    [
        new("None", StereoMode.None),
        new("Stereo ABC", StereoMode.StereoAbc),
        new("Stereo ACB", StereoMode.StereoAcb),
    ];

    public ObservableCollection<GamepadController> GamepadControllers { get; }

    private Theme _theme;
    public Theme Theme
    {
        get => _theme;
        set => this.RaiseAndSetIfChanged(ref _theme, value);
    }

    private ComputerType _computerType;
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

    private RomType _romType;
    public RomType RomType
    {
        get => _romType;
        set => this.RaiseAndSetIfChanged(ref _romType, value);
    }

    private JoystickType _joystickType = JoystickType.None;
    public JoystickType JoystickType
    {
        get => _joystickType;
        set => this.RaiseAndSetIfChanged(ref _joystickType, value);
    }

    private bool _emulateUsingKeyboard;
    public bool EmulateUsingKeyboard
    {
        get => _emulateUsingKeyboard;
        set => this.RaiseAndSetIfChanged(ref _emulateUsingKeyboard, value);
    }

    private Guid _gamepadControllerId = GamepadController.None.ControllerId;
    public Guid GamepadControllerId
    {
        get => _gamepadControllerId;
        set => this.RaiseAndSetIfChanged(ref _gamepadControllerId, value);
    }

    private bool _isTimeMachineEnabled;
    public bool IsTimeMachineEnabled
    {
        get => _isTimeMachineEnabled;
        set => this.RaiseAndSetIfChanged(ref _isTimeMachineEnabled, value);
    }

    private double _snapshotInterval;
    public double SnapshotInterval
    {
        get => _snapshotInterval;
        set => this.RaiseAndSetIfChanged(ref _snapshotInterval, value);
    }

    private double _maxDuration;
    public double MaxDuration
    {
        get => _maxDuration;
        set => this.RaiseAndSetIfChanged(ref _maxDuration, value);
    }

    private bool _isResumeEnabled;
    public bool IsResumeEnabled
    {
        get => _isResumeEnabled;
        set => this.RaiseAndSetIfChanged(ref _isResumeEnabled, value);
    }

    private bool _shouldIncludeTapeInResume;
    public bool ShouldIncludeTapeInResume
    {
        get => _shouldIncludeTapeInResume;
        set => this.RaiseAndSetIfChanged(ref _shouldIncludeTapeInResume, value);
    }

    private bool _shouldIncludeTimeMachineInResume;
    public bool ShouldIncludeTimeMachineInResume
    {
        get => _shouldIncludeTimeMachineInResume;
        set => this.RaiseAndSetIfChanged(ref _shouldIncludeTimeMachineInResume, value);
    }

    private bool _isTapeSaveEnabled;
    public bool IsTapeSaveEnabled
    {
        get => _isTapeSaveEnabled;
        set => this.RaiseAndSetIfChanged(ref _isTapeSaveEnabled, value);
    }

    private TapeSpeed _tapeSaveSpeed = TapeSpeed.Normal;
    public TapeSpeed TapeSaveSpeed
    {
        get => _tapeSaveSpeed;
        set => this.RaiseAndSetIfChanged(ref _tapeSaveSpeed, value);
    }

    private bool _isBeeperEnabled;
    public bool IsBeeperEnabled
    {
        get => _isBeeperEnabled;
        set => this.RaiseAndSetIfChanged(ref _isBeeperEnabled, value);
    }

    private bool _isAyEnabled;
    public bool IsAyEnabled
    {
        get => _isAyEnabled;
        set => this.RaiseAndSetIfChanged(ref _isAyEnabled, value);
    }

    private bool _isAySupportedStandardSpectrum;
    public bool IsAySupportedStandardSpectrum
    {
        get => _isAySupportedStandardSpectrum;
        set => this.RaiseAndSetIfChanged(ref _isAySupportedStandardSpectrum, value);
    }

    private StereoMode _stereoMode;
    public StereoMode StereoMode
    {
        get => _stereoMode;
        set => this.RaiseAndSetIfChanged(ref _stereoMode, value);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _gamepadManager.ControllerChanged -= GamepadManagerOnControllerChanged;
        GamepadMappingViewModel.Dispose();
    }
}
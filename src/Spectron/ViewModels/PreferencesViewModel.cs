using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.GamePad;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Models;
using OldBit.Spectron.Settings;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class PreferencesViewModel : ViewModelBase
{
    private readonly GamePadManager _gamePadManager;

    private GamePadSettings _gamePad1Settings;
    private GamePadSettings _gamePad2Settings;

    public ReactiveCommand<Unit, Preferences> UpdatePreferencesCommand { get; }
    public ReactiveCommand<JoystickId, Task> OpenGamePadMappingCommand { get; }

    public Interaction<GamePadMappingViewModel, GamePadSettings?> ShowGamePadMappingView { get; }

    public PreferencesViewModel(Preferences preferences, GamePadManager gamePadManager)
    {
        _gamePadManager = gamePadManager;

        ComputerType = preferences.ComputerType;
        IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
        RomType = preferences.RomType;

        JoystickKeyboardType = preferences.Joystick.JoystickKeyboardType;
        Joystick1Type = preferences.Joystick.Joystick1Type;
        Joystick2Type = preferences.Joystick.Joystick2Type;
        Joystick1GamePad = preferences.Joystick.Joystick1GamePad;
        Joystick2GamePad = preferences.Joystick.Joystick2GamePad;
        _gamePad1Settings = preferences.Joystick.GamePad1Settings;
        _gamePad2Settings = preferences.Joystick.GamePad2Settings;

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
        OpenGamePadMappingCommand = ReactiveCommand.Create<JoystickId, Task>(OpenGamePadMapping);

        ShowGamePadMappingView = new Interaction<GamePadMappingViewModel, GamePadSettings?>();
    }

    private Preferences UpdatePreferences() => new()
    {
        ComputerType = ComputerType,
        IsUlaPlusEnabled = IsUlaPlusEnabled,
        RomType = RomType,
        Joystick = new JoystickSettings
        {
            JoystickKeyboardType = JoystickKeyboardType,
            Joystick1Type = Joystick1Type,
            Joystick2Type = Joystick2Type,
            Joystick1GamePad = Joystick1GamePad,
            Joystick2GamePad = Joystick2GamePad,
            GamePad1Settings = _gamePad1Settings,
            GamePad2Settings = _gamePad2Settings,
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

    private async Task OpenGamePadMapping(JoystickId joystick)
    {
        var (gamePadControllerId, gamePadSettings) = joystick switch
        {
            JoystickId.Joystick1 => (Joystick1GamePad, _gamePad1Settings),
            JoystickId.Joystick2 => (Joystick2GamePad, _gamePad2Settings),
            _ => (Guid.Empty, null)
        };

        var gamePadController = _gamePadManager.GamePadControllers.FirstOrDefault(x => x.Id == gamePadControllerId);

        if (gamePadController == null)
        {
            return;
        }

        var viewModel = new GamePadMappingViewModel(gamePadController, _gamePadManager, gamePadSettings!);
        gamePadSettings = await ShowGamePadMappingView.Handle(viewModel);

        if (gamePadSettings == null)
        {
            return;
        }

        switch (joystick)
        {
            case JoystickId.Joystick1:
                _gamePad1Settings = gamePadSettings;
                break;

            case JoystickId.Joystick2:
                _gamePad2Settings = gamePadSettings;
                break;
        }
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

    public ObservableCollection<GamePadController> GamePadControllers => _gamePadManager.GamePadControllers;

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

    private JoystickType _joystickKeyboardType = JoystickType.None;
    public JoystickType JoystickKeyboardType
    {
        get => _joystickKeyboardType;
        set => this.RaiseAndSetIfChanged(ref _joystickKeyboardType, value);
    }

    private JoystickType _joystick1Type = JoystickType.None;
    public JoystickType Joystick1Type
    {
        get => _joystick1Type;
        set => this.RaiseAndSetIfChanged(ref _joystick1Type, value);
    }


    private JoystickType _joystick2Type = JoystickType.None;
    public JoystickType Joystick2Type
    {
        get => _joystick2Type;
        set => this.RaiseAndSetIfChanged(ref _joystick2Type, value);
    }

    private Guid _joystick1GamePad = Guid.Empty;
    public Guid Joystick1GamePad
    {
        get => _joystick1GamePad;
        set => this.RaiseAndSetIfChanged(ref _joystick1GamePad, value);
    }

    private Guid _joystick2GamePad = Guid.Empty;
    public Guid Joystick2GamePad
    {
        get => _joystick2GamePad;
        set => this.RaiseAndSetIfChanged(ref _joystick2GamePad, value);
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
}
using System;
using System.Collections.Generic;
using System.Reactive;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Settings;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class PreferencesViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Preferences> UpdatePreferencesCommand { get; }

    public PreferencesViewModel(Preferences preferences)
    {
        ComputerType = preferences.ComputerType;
        IsUlaPlusEnabled = preferences.IsUlaPlusEnabled;
        RomType = preferences.RomType;

        JoystickKeyboardType = preferences.Joystick.JoystickKeyboardType;
        Joystick1Type = preferences.Joystick.Joystick1Type;
        Joystick2Type = preferences.Joystick.Joystick2Type;

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

        UpdatePreferencesCommand = ReactiveCommand.Create(() => new Preferences
        {
            ComputerType = ComputerType,
            IsUlaPlusEnabled = IsUlaPlusEnabled,
            RomType = RomType,
            Joystick = new JoystickSettings
            {
                JoystickKeyboardType = JoystickKeyboardType,
                Joystick1Type = Joystick1Type,
                Joystick2Type = Joystick2Type
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
        });
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
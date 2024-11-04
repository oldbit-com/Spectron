using System;
using System.Collections.Generic;
using System.Reactive;
using OldBit.Spectron.Emulation;
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
        JoystickType = preferences.Joystick.JoystickType;
        JoystickUseCursorKeys = preferences.Joystick.UseCursorKeys;
        IsResumeEnabled = preferences.IsResumeEnabled;

        IsBeeperEnabled = preferences.AudioSettings.IsBeeperEnabled;
        IsAyAudioEnabled = preferences.AudioSettings.IsAyAudioEnabled;
        IsAyAudioEnabled48K = preferences.AudioSettings.IsAyAudioEnabled48K;

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
                JoystickType = JoystickType,
                UseCursorKeys = JoystickUseCursorKeys,
            },
            IsResumeEnabled = IsResumeEnabled,

            AudioSettings = new AudioSettings
            {
                IsBeeperEnabled = IsBeeperEnabled,
                IsAyAudioEnabled = IsAyAudioEnabled,
                IsAyAudioEnabled48K = IsAyAudioEnabled48K
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

    private bool _joystickUseCursorKeys;
    public bool JoystickUseCursorKeys
    {
        get => _joystickUseCursorKeys;
        set => this.RaiseAndSetIfChanged(ref _joystickUseCursorKeys, value);
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

    public List<NameValuePair<TapeSpeed>> TapeSpeeds { get; } =
    [
        new("Normal", TapeSpeed.Normal),
        new("Accelerated", TapeSpeed.Accelerated),
        new("Instant", TapeSpeed.Instant)
    ];

    private bool _isBeeperEnabled;
    public bool IsBeeperEnabled
    {
        get => _isBeeperEnabled;
        set => this.RaiseAndSetIfChanged(ref _isBeeperEnabled, value);
    }

    private bool _isAyAudioEnabled;
    public bool IsAyAudioEnabled
    {
        get => _isAyAudioEnabled;
        set => this.RaiseAndSetIfChanged(ref _isAyAudioEnabled, value);
    }

    private bool _isAyAudioEnabled48K;
    public bool IsAyAudioEnabled48K
    {
        get => _isAyAudioEnabled48K;
        set => this.RaiseAndSetIfChanged(ref _isAyAudioEnabled48K, value);
    }
}
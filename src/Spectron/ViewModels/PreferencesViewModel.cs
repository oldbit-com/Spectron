using System;
using System.Reactive;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Rom;
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

        IsTimeMachineEnabled = preferences.TimeMachine.IsEnabled;
        SnapshotInterval = preferences.TimeMachine.SnapshotInterval.TotalSeconds;
        MaxDuration = preferences.TimeMachine.MaxDuration.TotalSeconds;

        IsTapeSaveEnabled = preferences.TapeSaving.IsEnabled;
        IsFastTapeSaveEnabled = preferences.TapeSaving.IsFastSaveEnabled;

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

            TimeMachine = new TimeMachineSettings
            {
                IsEnabled = IsTimeMachineEnabled,
                SnapshotInterval = TimeSpan.FromSeconds(SnapshotInterval),
                MaxDuration = TimeSpan.FromSeconds(MaxDuration)
            },

            TapeSaving = new TapeSavingSettings(IsTapeSaveEnabled, IsFastTapeSaveEnabled)
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

    private bool _isFastTapeSaveEnabled;
    public bool IsFastTapeSaveEnabled
    {
        get => _isFastTapeSaveEnabled;
        set => this.RaiseAndSetIfChanged(ref _isFastTapeSaveEnabled, value);
    }
}
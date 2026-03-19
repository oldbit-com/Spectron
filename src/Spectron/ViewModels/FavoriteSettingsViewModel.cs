using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.ViewModels;

public partial class FavoriteSettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private NameValuePair<ComputerType?> _computerType;

    [ObservableProperty]
    private NameValuePair<JoystickType?> _joystickType;

    [ObservableProperty]
    private NameValuePair<MouseType?> _mouseType;

    [ObservableProperty]
    private NameValuePair<TapeSpeed?> _tapeLoadSpeed;

    [ObservableProperty]
    private bool? _isUlaPlusEnabled;

    [ObservableProperty]
    private bool? _isAyEnabled;

    public FavoriteSettingsViewModel(
        ComputerType? computerType = null,
        JoystickType? joystickType = null,
        MouseType? mouseType = null,
        TapeSpeed? tapeLoadingSpeed = null,
        bool? isUlaPlusEnabled = null,
        bool? isAyEnabled = null)
    {
        ComputerType = ComputerTypes.FirstOrDefault(x => x.Value == computerType, ComputerTypes[0]);
        JoystickType = JoystickTypes.FirstOrDefault(x => x.Value == joystickType, JoystickTypes[0]);
        MouseType = MouseTypes.FirstOrDefault(x => x.Value == mouseType, MouseTypes[0]);
        TapeLoadSpeed = TapeLoadSpeeds.FirstOrDefault(x => x.Value == tapeLoadingSpeed, TapeLoadSpeeds[0]);
        IsUlaPlusEnabled = isUlaPlusEnabled;
        IsAyEnabled = isAyEnabled;
    }

    public List<NameValuePair<ComputerType?>> ComputerTypes { get; } =
    [
        new("Default", null),
        new("ZX Spectrum 16k", OldBit.Spectron.Emulation.ComputerType.Spectrum16K),
        new("ZX Spectrum 48k", OldBit.Spectron.Emulation.ComputerType.Spectrum48K),
        new("ZX Spectrum 128k", OldBit.Spectron.Emulation.ComputerType.Spectrum128K),
    ];

    public List<NameValuePair<JoystickType?>> JoystickTypes { get; } =
    [
        new("Default", null),
        new("None", OldBit.Spectron.Emulation.Devices.Joystick.JoystickType.None),
        new("Kempston", OldBit.Spectron.Emulation.Devices.Joystick.JoystickType.Kempston),
        new("Sinclair 1", OldBit.Spectron.Emulation.Devices.Joystick.JoystickType.Sinclair1),
        new("Sinclair 2", OldBit.Spectron.Emulation.Devices.Joystick.JoystickType.Sinclair2),
        new("Cursor", OldBit.Spectron.Emulation.Devices.Joystick.JoystickType.Cursor),
        new("Fuller", OldBit.Spectron.Emulation.Devices.Joystick.JoystickType.Fuller),
    ];

    public List<NameValuePair<MouseType?>> MouseTypes { get; } =
    [
        new("Default", null),
        new("None", OldBit.Spectron.Emulation.Devices.Mouse.MouseType.None),
        new("Kempston", OldBit.Spectron.Emulation.Devices.Mouse.MouseType.Kempston),
    ];

    public List<NameValuePair<TapeSpeed?>> TapeLoadSpeeds { get; } =
    [
        new("Default", null),
        new("Normal", TapeSpeed.Normal),
        new("Instant", TapeSpeed.Instant),
        new("Instant", TapeSpeed.Accelerated),
    ];
}
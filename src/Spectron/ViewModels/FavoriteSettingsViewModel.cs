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
    public partial NameValuePair<ComputerType?> ComputerType { get; set; }

    [ObservableProperty]
    public partial NameValuePair<JoystickType?> JoystickType { get; set; }

    [ObservableProperty]
    public partial NameValuePair<MouseType?> MouseType { get; set; }

    [ObservableProperty]
    public partial NameValuePair<TapeSpeed?> TapeLoadSpeed { get; set; }

    [ObservableProperty]
    public partial bool? IsUlaPlusEnabled { get; set; }

    [ObservableProperty]
    public partial bool? IsAyEnabled { get; set; }

    [ObservableProperty]
    public partial bool? IsInterface1Enabled { get; set; }

    [ObservableProperty]
    public partial bool? IsBeta128Enabled { get; set; }

    [ObservableProperty]
    public partial bool? IsDivMmcEnabled { get; set; }

    public FavoriteSettingsViewModel(
        ComputerType? computerType = null,
        JoystickType? joystickType = null,
        MouseType? mouseType = null,
        TapeSpeed? tapeLoadingSpeed = null,
        bool? isUlaPlusEnabled = null,
        bool? isAyEnabled = null,
        bool? isInterface1Enabled = null,
        bool? isBeta128Enabled = null,
        bool? isDivMmcEnabled = null)
    {
        ComputerType = ComputerTypes.FirstOrDefault(x => x.Value == computerType, ComputerTypes[0]);
        JoystickType = JoystickTypes.FirstOrDefault(x => x.Value == joystickType, JoystickTypes[0]);
        MouseType = MouseTypes.FirstOrDefault(x => x.Value == mouseType, MouseTypes[0]);
        TapeLoadSpeed = TapeLoadSpeeds.FirstOrDefault(x => x.Value == tapeLoadingSpeed, TapeLoadSpeeds[0]);
        IsUlaPlusEnabled = isUlaPlusEnabled;
        IsAyEnabled = isAyEnabled;
        IsInterface1Enabled = isInterface1Enabled;
        IsBeta128Enabled = isBeta128Enabled;
        IsDivMmcEnabled = isDivMmcEnabled;
    }

    public List<NameValuePair<ComputerType?>> ComputerTypes { get; } =
    [
        new("Default", null),
        new("ZX Spectrum 16k", OldBit.Spectron.Emulation.ComputerType.Spectrum16K),
        new("ZX Spectrum 48k", OldBit.Spectron.Emulation.ComputerType.Spectrum48K),
        new("ZX Spectrum 128k", OldBit.Spectron.Emulation.ComputerType.Spectrum128K),
        new("Timex Computer 2048", OldBit.Spectron.Emulation.ComputerType.Timex2048),
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
        new("Accelerated", TapeSpeed.Accelerated),
    ];
}
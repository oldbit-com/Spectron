using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Models;

namespace OldBit.Spectron.Preferences;

public class DefaultSettings
{
    public bool IsUlaPlusEnabled { get; set; }

    public BorderSize BorderSize { get; set; } = BorderSize.Medium;

    public ComputerType ComputerType { get; set; } = ComputerType.Spectrum48K;

    public RomType RomType { get; set; } = RomType.Original;

    public JoystickType JoystickType { get; set; } = JoystickType.None;

    public int MaxRecentFiles { get; set; } = 10;

    public TapeLoadingSpeed TapeLoadingSpeed { get; set; } = TapeLoadingSpeed.Instant;
}
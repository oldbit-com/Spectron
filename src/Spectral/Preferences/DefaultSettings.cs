using OldBit.Spectral.Emulation;
using OldBit.Spectral.Emulation.Devices.Joystick;
using OldBit.Spectral.Emulation.Rom;
using OldBit.Spectral.Models;

namespace OldBit.Spectral.Preferences;

public class DefaultSettings
{
    public bool IsUlaPlusEnabled { get; set; }

    public BorderSize BorderSize { get; set; } = BorderSize.Medium;

    public ComputerType ComputerType { get; set; } = ComputerType.Spectrum48K;

    public RomType RomType { get; set; } = RomType.Original;

    public JoystickType JoystickType { get; set; } = JoystickType.None;

    public int MaxRecentFiles { get; set; } = 10;
}
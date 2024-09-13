using OldBit.Spectron.Emulation.Devices.Joystick;

namespace OldBit.Spectron.Settings;

public record JoystickSettings
{
    public JoystickType JoystickType { get; init; } = JoystickType.None;

    public bool UseCursorKeys { get; init; } = true;
}
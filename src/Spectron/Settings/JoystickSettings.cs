using OldBit.Spectron.Emulation.Devices.Joystick;

namespace OldBit.Spectron.Settings;

public record JoystickSettings
{
    public JoystickType JoystickKeyboardType { get; init; } = JoystickType.None;

    public JoystickType Joystick1Type { get; init; } = JoystickType.None;

    public JoystickType Joystick2Type { get; init; } = JoystickType.None;
}
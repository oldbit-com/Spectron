using System;
using OldBit.Spectron.Emulation.Devices.Joystick;

namespace OldBit.Spectron.Settings;

public record JoystickSettings
{
    public JoystickType JoystickKeyboardType { get; init; } = JoystickType.None;

    public JoystickType JoystickType { get; init; } = JoystickType.None;

    public Guid JoystickGamepad { get; init; } = Guid.Empty;

    public GamepadSettings GamepadSettings { get; init; } = new();
}
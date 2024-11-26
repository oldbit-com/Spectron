using System;
using OldBit.Spectron.Emulation.Devices.Joystick;

namespace OldBit.Spectron.Settings;

public record JoystickSettings
{
    public JoystickType JoystickKeyboardType { get; init; } = JoystickType.None;

    public JoystickType Joystick1Type { get; init; } = JoystickType.None;

    public JoystickType Joystick2Type { get; init; } = JoystickType.None;

    public Guid Joystick1Gamepad { get; init; } = Guid.Empty;

    public Guid Joystick2Gamepad { get; init; } = Guid.Empty;

    public GamepadSettings Gamepad1Settings { get; init; } = new();

    public GamepadSettings Gamepad2Settings { get; init; } = new();
}
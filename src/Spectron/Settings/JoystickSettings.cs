using System;
using OldBit.Spectron.Emulation.Devices.Joystick;
using SharpHook.Data;

namespace OldBit.Spectron.Settings;

public record JoystickSettings
{
    public JoystickType JoystickType { get; init; } = JoystickType.None;

    public Guid GamepadControllerId { get; init; } = Guid.Empty;

    public GamepadSettings GamepadSettings { get; init; } = new();

    public bool EmulateUsingKeyboard { get; init; } = true;

    public KeyCode FireKey { get; init; } = KeyCode.VcSpace;
}
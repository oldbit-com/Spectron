using System;
using Avalonia.Input;
using OldBit.Spectron.Emulation.Devices.Joystick;

namespace OldBit.Spectron.Settings;

public record JoystickSettings
{
    public JoystickType JoystickType { get; init; } = JoystickType.None;

    public Guid GamepadControllerId { get; init; } = Guid.Empty;

    public GamepadSettings GamepadSettings { get; init; } = new();

    public bool EmulateUsingKeyboard { get; init; } = true;

    public Key FireKey { get; init; } = Key.Space;
}
using OldBit.JoyPad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public record GamepadButton(
    int Id,
    string Name,
    DirectionalPadDirection Direction = DirectionalPadDirection.None);
using OldBit.JoyPad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public record GamepadButton(
    int ButtonId,
    string Name,
    DirectionalPadDirection Direction = DirectionalPadDirection.None);
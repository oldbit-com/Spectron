using OldBit.Joypad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Gamepad;

public record GamepadButton(
    int ButtonId,
    string Name,
    DirectionalPadDirection Direction = DirectionalPadDirection.None);
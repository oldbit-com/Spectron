using OldBit.Joypad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public class ValueChangedEventArgs(int controlId, bool isPressed, DirectionalPadDirection direction) : EventArgs
{
    public int ControlId { get; } = controlId;

    public bool IsPressed { get; } = isPressed;

    public DirectionalPadDirection Direction { get; } = direction;
}
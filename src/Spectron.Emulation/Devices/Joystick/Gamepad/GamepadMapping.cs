using OldBit.JoyPad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public sealed record GamepadMapping
{
    public GamepadMapping()
    {
    }

    public GamepadMapping(GamepadButton button, GamepadAction action)
    {
        ButtonId = button.ButtonId;
        Direction = button.Direction;
        Action = action;
    }

    public int ButtonId { get; init; }

    public GamepadAction Action { get; init; }

    public DirectionalPadDirection Direction { get; init; }
}
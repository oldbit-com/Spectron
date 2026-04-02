using OldBit.Joypad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Gamepad;

public sealed record GamepadMapping
{
    // ReSharper disable once UnusedMember.Global
    public GamepadMapping()
    {
    }

    public GamepadMapping(GamepadButton button, GamepadAction action)
    {
        ControlId = button.ButtonId;
        Direction = button.Direction;
        Action = action;
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public int ControlId { get; init; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public GamepadAction Action { get; init; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public DirectionalPadDirection Direction { get; init; }
}
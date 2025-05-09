using System.Diagnostics.CodeAnalysis;
using OldBit.Joypad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Gamepad;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed record GamepadMapping
{
    public GamepadMapping()
    {
    }

    public GamepadMapping(GamepadButton button, GamepadAction action)
    {
        ControlId = button.ButtonId;
        Direction = button.Direction;
        Action = action;
    }

    public int ControlId { get; init; }

    public GamepadAction Action { get; init; }

    public DirectionalPadDirection Direction { get; init; }
}
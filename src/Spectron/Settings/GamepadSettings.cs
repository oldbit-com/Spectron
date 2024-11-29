using System;
using System.Collections.Generic;
using OldBit.JoyPad.Controls;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

namespace OldBit.Spectron.Settings;

public record GamepadSettings
{
    public Dictionary<Guid, List<GamepadMapping>> MappingsByController { get; set; } = new();
}

public record GamepadMapping
{
    public GamepadMapping()
    {
    }

    public GamepadMapping(GamepadButton button, GamepadAction action)
    {
        ButtonId = button.Id;
        Direction = button.Direction;
        Action = action;
    }

    public int ButtonId { get; init; }

    public GamepadAction Action { get; init; }

    public DirectionalPadDirection Direction { get; init; }
}
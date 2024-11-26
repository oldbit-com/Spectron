using System.Collections.Generic;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

namespace OldBit.Spectron.Settings;

public record GamepadMapping(int ButtonId, GamepadAction Action);

public record GamepadSettings
{
    public List<GamepadMapping> Mappings { get; init; } = [];
}
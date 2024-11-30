using System;
using System.Collections.Generic;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

namespace OldBit.Spectron.Settings;

public record GamepadSettings
{
    public Dictionary<Guid, List<GamepadMapping>> Mappings { get; } = new();
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

namespace OldBit.Spectron.Settings;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public record GamepadSettings
{
    public Dictionary<Guid, List<GamepadMapping>> Mappings { get; init; } = new();
}
using System.Collections.Generic;
using OldBit.Spectron.Emulation.Devices.Joystick.GamePad;

namespace OldBit.Spectron.Settings;

public record GamePadMapping(int ButtonId, GamePadAction Action);

public record GamePadSettings
{
    public List<GamePadMapping> Mappings { get; init; } = [];
}
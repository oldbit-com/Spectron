using System.Collections.Generic;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.Settings;

public class FavoriteProgram
{
    public string Path { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool IsFolder { get; init; }
    public List<FavoriteProgram> Items { get; init; } = [];
    public ComputerType? ComputerType { get; init; }
    public JoystickType? JoystickType { get; init; }
    public MouseType? MouseType { get; init; }
    public TapeSpeed? TapeLoadSpeed { get; init; }
    public bool? IsUlaPlusEnabled { get; init; }
    public bool? IsAyEnabled { get; init; }
    public bool? IsInterface1Enabled { get; init; }
    public bool? IsBeta128Enabled { get; init; }
    public bool? IsDivMmcEnabled { get; init; }
}

public class FavoritePrograms
{
    public List<FavoriteProgram> Items { get; init; } = [];
}
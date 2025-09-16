using OldBit.Spectron.Emulation.Devices.Interface1;

namespace OldBit.Spectron.Settings;

public record Interface1Settings
{
    public bool IsEnabled { get; init; }

    public Interface1RomVersion RomVersion { get; init; } = Interface1RomVersion.V2;

    public int ConnectedMicrodrivesCount { get; init; } = 2;
}
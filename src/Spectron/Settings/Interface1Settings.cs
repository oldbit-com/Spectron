using OldBit.Spectron.Emulation.Devices.Interface1;

namespace OldBit.Spectron.Settings;

public record Interface1Settings
{
    public bool IsEnabled { get; init; }

    public RomVersion RomVersion { get; init; } = RomVersion.V2;

    public int ConnectedMicrodrivesCount { get; init; } = 2;
}
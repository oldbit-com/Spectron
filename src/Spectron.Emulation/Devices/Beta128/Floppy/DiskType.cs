namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

internal static class DiskType
{
    private const byte EightyTracksDoubleSided = 0x16;
    private const byte FortyTracksDoubleSided = 0x17;
    private const byte EightyTracksSingleSided = 0x18;
    private const byte FortyTracksSingleSided = 0x19;

    internal static byte GetDiskType(int totalTracks, int totalSides) =>
        (tracks: totalTracks, sides: totalSides) switch
        {
            (80, 2) => EightyTracksDoubleSided,
            (80, 1) => EightyTracksSingleSided,
            (40, 2) => FortyTracksDoubleSided,
            (40, 1) => FortyTracksSingleSided,
            _ => throw new ArgumentException("Invalid disk type"),
        };
}
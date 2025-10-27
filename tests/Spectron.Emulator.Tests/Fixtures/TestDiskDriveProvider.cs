using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;

namespace OldBit.Spectron.Emulator.Tests.Fixtures;

public class TestDiskDriveProvider : IDiskDriveProvider
{
    public Dictionary<DriveId, DiskDrive> Drives { get; } = new();

    public TestDiskDriveProvider()
    {
        foreach (var drive in Enum.GetValues<DriveId>())
        {
            Drives[drive] = new DiskDrive(drive);
        }
    }
}
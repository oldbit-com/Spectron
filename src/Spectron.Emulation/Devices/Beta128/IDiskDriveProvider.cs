using OldBit.Spectron.Emulation.Devices.Beta128.Drive;

namespace OldBit.Spectron.Emulation.Devices.Beta128;

public interface IDiskDriveProvider
{
    Dictionary<DriveId, DiskDrive> Drives { get; }

    DiskDrive? ActiveDrive { get; }
}
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;

namespace OldBit.Spectron.Emulation.Devices.Beta128;

public class DiskDriveManager : IDiskDriveProvider
{
    public Dictionary<DriveId, DiskDrive> Drives { get; } = new();

    public DiskDrive? ActiveDrive { get; }

    public DiskDrive this[DriveId drive] => Drives[drive];

    public DiskDriveManager()
    {
        foreach (var drive in Enum.GetValues<DriveId>())
        {
            var diskDrive = new DiskDrive(drive);
            Drives[drive] = diskDrive;
        }
    }
}
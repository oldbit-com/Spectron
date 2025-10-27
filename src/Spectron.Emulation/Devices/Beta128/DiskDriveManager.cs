using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Beta128.Events;

namespace OldBit.Spectron.Emulation.Devices.Beta128;

public class DiskDriveManager : IDiskDriveProvider
{
    public Dictionary<DriveId, DiskDrive> Drives { get; } = new();

    public DiskDrive this[DriveId drive] => Drives[drive];

    public event DiskChangedEvent? DiskChanged;
    public event DiskActivityEvent? DiskActivity;

    public DiskDriveManager()
    {
        foreach (var drive in Enum.GetValues<DriveId>())
        {
            var diskDrive = new DiskDrive(drive);
            Drives[drive] = diskDrive;

            diskDrive.DiskChanged += e => DiskChanged?.Invoke(e);
        }
    }

    internal void OnDiskActivity() => DiskActivity?.Invoke(EventArgs.Empty);
}
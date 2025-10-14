using OldBit.Spectron.Emulation.Devices.Beta128.Events;
using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;
using OldBit.Spectron.Emulation.Devices.Beta128.Image;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Drive;

public sealed class DiskDrive(DriveId driveId)
{
    private const int MaxCylinders = 80;
    internal const int Rps = 5;

    public bool IsDiskInserted => Image?.Floppy != null;
    internal bool IsTrackZero => CylinderNo == 0;

    internal byte CylinderNo { get; private set; }
    internal Track? Track { get; private set; }

    internal bool IsSpinning => SpinTime > 0;
    internal long SpinTime { get; private set; }

    public DiskImage? Image { get; private set; }

    public bool IsWriteProtected { get; set; }

    public event DiskChangedEvent? DiskChanged;

    public void InsertDisk(string filePath)
    {
        Image = new DiskImage(filePath);

        OnDiskChanged();
    }

    internal void InsertDisk(string? filePath, DiskImageType diskImageType, bool isWriteProtected, ReadOnlySpan<byte> data)
    {
        Image = new DiskImage(filePath, diskImageType, data);
        IsWriteProtected = isWriteProtected;

        OnDiskChanged();
    }

    public void EjectDisk()
    {
        Image = null;

        OnDiskChanged();
    }

    internal void Spin(long spinTime) => SpinTime = spinTime;

    internal void Stop() => SpinTime = 0;

    internal void Seek(byte cylinderNo, byte sideNo)
    {
        CylinderNo = cylinderNo;
        Track = Image?.Floppy.GetTrack(CylinderNo, sideNo);
    }

    internal void Step(int stepIncrement)
    {
        int cylinderNo = CylinderNo;
        cylinderNo += stepIncrement;

        if (cylinderNo < 0)
        {
            cylinderNo = 0;
        }

        if (cylinderNo >= MaxCylinders)
        {
            cylinderNo = MaxCylinders;
        }

        CylinderNo = (byte)cylinderNo;
    }

    private void OnDiskChanged() => DiskChanged?.Invoke(new DiskChangedEventArgs { DriveId = driveId });

}
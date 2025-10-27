using OldBit.Spectron.Emulation.Devices.Beta128.Events;
using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;
using OldBit.Spectron.Emulation.Devices.Beta128.Image;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Drive;

public sealed class DiskDrive(DriveId driveId)
{
    private const int MaxCylinders = 80;
    internal const int Rps = 5;

    public bool IsDiskInserted => DiskFile?.Floppy != null;
    internal bool IsTrackZero => CylinderNo == 0;

    internal byte CylinderNo { get; private set; }
    internal Track? Track { get; private set; }

    internal bool IsMotorOn => SpinTime > 0;
    internal long SpinTime { get; private set; }

    public DiskFile? DiskFile { get; private set; }

    public bool IsWriteProtected { get; set; }

    public event DiskChangedEvent? DiskChanged;

    public void NewDisk()
    {
        DiskFile = new DiskFile();

        OnDiskChanged();
    }

    public void InsertDisk(string filePath)
    {
        DiskFile = new DiskFile(filePath);

        OnDiskChanged();
    }

    public void InsertDisk(string filePath, Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        var diskImageType = DiskImage.GetImageType(filePath);
        InsertDisk(filePath, diskImageType, false, memoryStream.ToArray());

        OnDiskChanged();
    }

    internal void InsertDisk(string? filePath, DiskImageType diskImageType, bool isWriteProtected, ReadOnlySpan<byte> data)
    {
        DiskFile = new DiskFile(filePath, diskImageType, data);
        IsWriteProtected = isWriteProtected;

        OnDiskChanged();
    }

    public void EjectDisk()
    {
        DiskFile = null;

        OnDiskChanged();
    }

    internal void MotorOn(long motorTime) => SpinTime = motorTime;

    internal void MotorOff() => SpinTime = 0;

    internal void SelectTrack(byte sideNo) => Track = DiskFile?.Floppy.GetTrack(CylinderNo, sideNo);

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
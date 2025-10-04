using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Drive;

public sealed class DiskDrive(DriveId driveId)
{
    private FloppyDisk? _floppy;

    private const int MaxCylinders = 80;
    internal const int Rps = 5;

    internal bool IsDiskLoaded => _floppy != null;
    internal bool IsTrackZero => CylinderNo == 0;

    internal byte CylinderNo { get; private set; }
    internal Track? Track { get; private set; }

    internal bool IsSpinning => SpinTime > 0;
    internal long SpinTime { get; private set; }

    public bool IsWriteProtected { get; set; }

    internal void InsertDisk(FloppyDisk floppy)
    {
        _floppy = floppy;
    }

    internal void Spin(long spinTime) => SpinTime = spinTime;

    internal void Stop() => SpinTime = 0;

    internal void Seek(byte cylinderNo, byte sideNo)
    {
        CylinderNo = cylinderNo;
        Track = _floppy?.GetTrack(CylinderNo, sideNo);
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
}
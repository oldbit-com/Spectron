using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Drive;

public sealed class DiskDrive
{
    private FloppyDisk? _floppy;

    internal const int Rps = 5;

    internal bool IsDiskLoaded => _floppy != null;
    internal bool IsTrackZero { get; private set; }
    internal bool IsWriteProtected { get; private set; }

    // TODO: Move to controller
    internal byte CylinderNo {get; set; }
    internal byte SideNo { get; set; }
    internal byte SectorNo { get; set; }

    internal Track? Track { get; private set; }

    internal bool IsSpinning => SpinTime > 0;
    internal long SpinTime { get; private set; }

    internal void InsertDisk(FloppyDisk floppy)
    {
        _floppy = floppy;
    }

    internal void Spin(long spinTime) => SpinTime = spinTime;

    internal void Stop() => SpinTime = 0;

    internal void Seek()
    {
        Track = _floppy?.GetTrack(CylinderNo, SideNo);
    }
}
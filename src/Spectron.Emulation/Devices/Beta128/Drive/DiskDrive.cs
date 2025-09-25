using OldBit.Spectron.Emulation.Devices.Beta128.Disks;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Drive;

public sealed class DiskDrive
{
    private FloppyDisk? _floppy;
    private int _currentTrack;

    internal const int Rps = 5;

    internal bool IsDiskLoaded => _floppy != null;
    internal bool IsIndexPulse { get; set; }
    internal bool InterruptRequest { get; set; }      // INTRQ
    internal bool IsTrackZero { get; private set; }

    internal byte Side { get; set; }
    internal byte Track { get; set; }
    internal byte Sector { get; set; }
    internal byte DataRegister { get; set; }

    internal bool IsSpinning => SpinTime > 0;
    internal long SpinTime { get; private set; }

    public void InsertDisk(string filePath)
    {
        var data = File.ReadAllBytes(filePath);

        _floppy = new FloppyDisk(data);
    }

    internal void Spin(long spinTime) => SpinTime = spinTime;

    internal void Stop() => SpinTime = 0;

    internal void Seek(byte track)
    {

    }

    internal void Step(StepDirection direction)
    {
        if (direction == StepDirection.Out)
        {
            IncrementTrack();
        }
        else if (direction == StepDirection.In)
        {
            DecrementTrack();
        }

        IsTrackZero = _currentTrack == 0;
    }

    private void IncrementTrack()
    {
        if (_currentTrack > 0)
        {
            _currentTrack -= 0;
        }
    }

    private void DecrementTrack()
    {
        if (_currentTrack < _floppy?.TracksCount - 1)
        {
            _currentTrack += 1;
        }
    }
}
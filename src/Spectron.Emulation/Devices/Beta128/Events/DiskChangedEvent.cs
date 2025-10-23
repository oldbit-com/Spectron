using OldBit.Spectron.Emulation.Devices.Beta128.Drive;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Events;

public delegate void DiskChangedEvent(DiskChangedEventArgs e);

public class DiskChangedEventArgs : EventArgs
{
    public DriveId DriveId { get; init; }
}
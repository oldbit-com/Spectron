namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrives.Events;

public delegate void CartridgeChangedEvent(CartridgeChangedEventArgs e);

public class CartridgeChangedEventArgs : EventArgs
{
    public MicrodriveId DriveId { get; init; }
}
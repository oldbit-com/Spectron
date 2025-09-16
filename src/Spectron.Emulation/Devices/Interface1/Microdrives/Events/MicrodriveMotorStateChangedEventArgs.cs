namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrives.Events;

public delegate void MicrodriveMotorStateChangedEvent(MicrodriveMotorStateChangedEventArgs e);

public class MicrodriveMotorStateChangedEventArgs(MicrodriveId driveId, bool isMotorOn) : EventArgs
{
    public bool IsMotorOn { get; } = isMotorOn;
    public MicrodriveId DriveId { get; } = driveId;
}
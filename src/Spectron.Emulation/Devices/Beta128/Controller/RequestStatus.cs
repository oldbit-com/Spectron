namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

[Flags]
public enum RequestStatus
{
    /// <summary>
    /// No pending request.
    /// </summary>
    None = 0,

    /// <summary>
    /// DRQ (Data Request)
    /// </summary>
    DataRequest = 0x40,

    /// <summary>
    /// INTRQ (Interrupt Request)
    /// </summary>
    InterruptRequest = 0x80,
}
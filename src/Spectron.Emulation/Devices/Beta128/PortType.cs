namespace OldBit.Spectron.Emulation.Devices.Beta128;

internal enum PortType
{
    None = 0,
    Command = 0x1F,
    Status = 0x1F,
    Track = 0x3F,
    Sector = 0x5F,
    Data = 0x7F,
    Control = 0xFF,
}
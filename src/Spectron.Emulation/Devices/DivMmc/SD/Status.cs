namespace OldBit.Spectron.Emulation.Devices.DivMmc.SD;

[Flags]
internal enum Status
{
    Idle = 0x01,
    IllegalCommand = 0x04,
}
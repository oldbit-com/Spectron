namespace OldBit.Spectron.Emulation.Devices.Printer;

[Flags]
public enum PrinterStatus : byte
{
    Ready = 0x01,

    PrinterDetected = 0x40,

    NewLine = 0x80,
}
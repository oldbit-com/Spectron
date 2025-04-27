namespace OldBit.Spectron.Emulation.Devices.Printer;

public class ZxPrinter : IDevice
{
    private const byte PrinterPort = 0xFB;

    public bool IsEnabled { get; set; }

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled || !IsPrinterPort(address))
        {
            return;
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled || !IsPrinterPort(address))
        {
            return null;
        }

        return 0xFF;
    }

    private static bool IsPrinterPort(Word address) => (address & 0xFF) == PrinterPort;
}
namespace OldBit.Spectron.Emulation.Devices.Printer;

public sealed class ZxPrinter : IDevice
{
    private const byte PrinterPort = 0xFB;
    private const int MaxPosition = 255;

    private const byte PrinterMotorBit = 0x04;
    private const byte PrinterPixelBit = 0x80;

    private int _stylusPosition = -1;
    private bool _isNewLine;

    public bool IsEnabled { get; set; }

    public List<DataRow> Rows { get; } = [];

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled || !IsPrinterPort(address))
        {
            return;
        }

        if (!IsMotorOn(value))
        {
            return;
        }

        // Before each line, printer settings are sent
        if (_stylusPosition == -1)
        {
            _stylusPosition = 0;
            _isNewLine = true;

            Rows.Add(new DataRow());

            return;
        }

        var pixel = (value & PrinterPixelBit) != 0;

        if (pixel)
        {
            Rows[^1].SetPixel(_stylusPosition);
        }

        _isNewLine = false;
        _stylusPosition += 1;

        if (_stylusPosition <= MaxPosition)
        {
            return;
        }

        _stylusPosition = -1;
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled || !IsPrinterPort(address))
        {
            return null;
        }

        var result = PrinterStatus.Ready;

        if (_isNewLine)
        {
            result |= PrinterStatus.NewLine;
        }

        return (byte)result;
    }

    private static bool IsPrinterPort(Word address) => (address & 0xFF) == PrinterPort;

    private static bool IsMotorOn(byte value) => (value & PrinterMotorBit) == 0;
}
namespace OldBit.Spectron.Emulation.Devices.Printer;

public class ZxPrinter : IDevice
{
    private const byte PrinterPort = 0xFB;
    private const int MaxPosition = 255;

    private const byte PrinterMotorBit = 0x04;
    private const byte PrinterPixelBit = 0x80;

    private int _stylusPosition = -1;
    private bool _isNewLine;
    private int _row;
    private List<DataRow> _rows = [];

    public bool IsEnabled { get; set; }

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

            return;
        }

        var pixel = (value & PrinterPixelBit) != 0;

        if (_stylusPosition < 8)
            Console.WriteLine($"Pos: {_stylusPosition} Row: {_row}  Pixel: {pixel}");

        _isNewLine = false;
        _stylusPosition += 1;

        if (_stylusPosition <= MaxPosition)
        {
            return;
        }

        _stylusPosition = -1;
        _row += 1;
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
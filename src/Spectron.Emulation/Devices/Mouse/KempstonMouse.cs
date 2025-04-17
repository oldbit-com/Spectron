namespace OldBit.Spectron.Emulation.Devices.Mouse;

public sealed class KempstonMouse : IDevice
{
    private byte _xAxis;
    private byte _yAxis;
    private MouseButtons _buttons = MouseButtons.None;

    internal bool IsEnabled { get; set; }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled)
        {
            return null;
        }

        if (IsXAxisPort(address))
        {
            return _xAxis;
        }

        if (IsYAxisPort(address))
        {
            return _yAxis;
        }

        if (IsButtonsPort(address))
        {
            return (byte)_buttons;
        }

        return null;
    }

    public void Update(byte xAxis, byte yAxis, MouseButtons buttons)
    {
        _xAxis = xAxis;
        _yAxis = yAxis;
        _buttons = buttons;
    }

    private static bool IsXAxisPort(Word address) => (address & 0b0000_0111_0010_0000) == 0b0000_0011_0000_0000;
    private static bool IsYAxisPort(Word address) => (address & 0b0000_0111_0010_0000) == 0b0000_0111_0000_0000;
    private static bool IsButtonsPort(Word address) => (address & 0b0000_0111_0010_0000) == 0b0000_0010_0000_0000;
}
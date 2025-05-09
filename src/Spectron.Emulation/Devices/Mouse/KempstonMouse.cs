namespace OldBit.Spectron.Emulation.Devices.Mouse;

public sealed class KempstonMouse : IMouse
{
    public MouseButtons Buttons { get; set; } = MouseButtons.None;

    public byte X { get; set; }

    public byte Y { get; set; }

    public byte? ReadPort(Word address)
    {
        if (IsXAxisPort(address))
        {
            return X;
        }

        if (IsYAxisPort(address))
        {
            return Y;
        }

        if (IsButtonsPort(address))
        {
            return (byte)Buttons;
        }

        return null;
    }

    private static bool IsXAxisPort(Word address) => (address & 0b0000_0111_0010_0000) == 0b0000_0011_0000_0000;
    private static bool IsYAxisPort(Word address) => (address & 0b0000_0111_0010_0000) == 0b0000_0111_0000_0000;
    private static bool IsButtonsPort(Word address) => (address & 0b0000_0111_0010_0000) == 0b0000_0010_0000_0000;
}
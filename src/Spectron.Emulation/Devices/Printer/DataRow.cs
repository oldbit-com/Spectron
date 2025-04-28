namespace OldBit.Spectron.Emulation.Devices.Printer;

internal sealed class DataRow
{
    internal byte[] Pixels { get; } = new byte[32];

    internal void SetPixel(int index)
    {
        var column = index / 8;
        var bit = index % 8;

        Pixels[column] |= (byte)(1 << bit);
    }
}
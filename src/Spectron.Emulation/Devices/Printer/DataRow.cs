using System.Collections;
using System.Text;

namespace OldBit.Spectron.Emulation.Devices.Printer;

public sealed class DataRow
{
    public byte[] Pixels { get; } = new byte[32];

    internal void SetPixel(int index)
    {
        var column = index / 8;
        var bit = 8 - index % 8;

        Pixels[column] |= (byte)(1 << bit);
    }

    public override string ToString()
    {
        var bits = new BitArray(Pixels);
        var sb = new StringBuilder(bits.Length);

        foreach (bool bit in bits)
        {
            sb.Append(bit ? '1' : '0');
        }

        return sb.ToString();
    }
}
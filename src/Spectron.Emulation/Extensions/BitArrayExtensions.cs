using System.Collections;

namespace OldBit.Spectron.Emulation.Extensions;

internal static class BitArrayExtensions
{
    internal static void Set(this BitArray bitArray, int index, int value) =>
        bitArray.Set(index, value != 0);

    internal static void Set(this BitArray bitArray, int endIndex,int startIndex,  int value)
    {
        for (var i = startIndex; i <= endIndex; i++)
        {
            bitArray.Set(i, value & 0x01);
            value >>= 1;
        }
    }

    internal static byte[] ToBytes(this BitArray bitArray)
    {
        var numBytes = (bitArray.Length + 7) / 8;
        var bytes = new byte[numBytes];

        bitArray.CopyTo(bytes, 0);

        return bytes.Reverse().ToArray();
    }
}
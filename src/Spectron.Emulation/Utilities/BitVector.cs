namespace OldBit.Spectron.Emulation.Utilities;

/// <summary>
/// Represents a vector that consists of bits so it can be used to manipulate bits in a byte array.
/// Array of bits is stored in reverse order: byte[0] represents the most significant bits.
/// </summary>
internal sealed class BitVector
{
    private readonly byte[] _buffer;

    internal BitVector(int bitCount) =>
        _buffer = new byte[(bitCount + 7) / 8];

    internal void Set(int fromBit, int toBit, long value)
    {
        if (fromBit < toBit)
        {
            (fromBit, toBit) = (toBit, fromBit);
        }

        for (var bit = toBit; bit <= fromBit; bit++)
        {
            var index = (8 * _buffer.Length - bit - 1) / 8;
            var bitPosition = bit % 8;
            var isSet = (value & 1) != 0;

            if (isSet)
            {
                _buffer[index] |= (byte)(1 << bitPosition);
            }
            else
            {
                _buffer[index] &= (byte)~(1 << bitPosition);
            }

            value >>= 1;
        }
    }

    internal byte[] ToArray() => _buffer;
}
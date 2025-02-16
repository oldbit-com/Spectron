namespace OldBit.Spectron.Disassembly.Helpers;

internal sealed class ByteDataReader(byte[] data, int address)
{
    internal int Address { get; private set; } = address;

    internal byte ReadeByte()
    {
        var value = data[Address];

        Address += 1;

        if (Address > 65535)
        {
            Address = 0;
        }

        return value;
    }

    internal byte PeekByte(int address) => data[address % 65536];

    internal Word ReadWord()
    {
        var lowByte = ReadeByte();
        var highByte = ReadeByte();

        return (Word)(lowByte | (highByte << 8));
    }

    internal IEnumerable<byte> GetRange(int start, int count)
    {
        for (var i = start; i < start + count; i++)
        {
            yield return data[i % 65536];
        }
    }
}
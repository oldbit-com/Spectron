namespace OldBit.Spectron.Disassembly.Helpers;

internal sealed class ByteDataReader(byte[] data, int address) : IDataReader
{
    public int Address { get; set; } = address;

    public byte ReadeByte()
    {
        var value = data[Address];

        Address += 1;

        if (Address > 65535)
        {
            Address = 0;
        }

        return value;
    }

    public byte PeekByte(int address) => data[address % 65536];

    public IEnumerable<byte> GetRange(int start, int count)
    {
        for (var i = start; i < start + count; i++)
        {
            yield return data[i % 65536];
        }
    }
}
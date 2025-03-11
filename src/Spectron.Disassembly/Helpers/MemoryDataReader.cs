using OldBit.Z80Cpu;

namespace OldBit.Spectron.Disassembly.Helpers;

internal class MemoryDataReader(IMemory memory, int address) : IDataReader
{
    public int Address { get; set; } = address;

    public byte ReadeByte()
    {
        var value = memory.Read((Word)Address);

        Address += 1;

        if (Address > 65535)
        {
            Address = 0;
        }

        return value;
    }

    public byte PeekByte(int address) => memory.Read((Word)(Address % 65536));

    public IEnumerable<byte> GetRange(int start, int count)
    {
        for (var i = start; i < start + count; i++)
        {
            yield return memory.Read((Word)(i % 65536));
        }
    }
}
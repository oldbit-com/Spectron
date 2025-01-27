using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Extensions;

public static class MemoryExtensions
{
    internal static List<byte> ReadBytes(this IMemory memory, Word startAddress, int count)
    {
        var bytes = new List<byte>();

        for (var i = 0; i < count; i++)
        {
            bytes.Add(memory.Read((Word)(startAddress + i)));
        }

        return bytes;
    }

    public static Word ReadWord(this IMemory memory, Word address) =>
        (Word)(memory.Read(address) | memory.Read((Word)(address + 1)) << 8);
}
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Extensions;

internal static class MemoryExtensions
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
}
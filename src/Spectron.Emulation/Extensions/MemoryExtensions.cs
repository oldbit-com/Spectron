using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Extensions;

public static class MemoryExtensions
{
    public static List<byte> ReadBytes(this IMemory memory, Word address, int count)
    {
        var end = address + count;

        switch (memory)
        {
            case Memory48K memory48:
                return memory48.Memory[address..end].ToList();

            case Memory16K memory16:
                return memory16.Memory[address..end].ToList();
        }

        var bytes = new List<byte>();

        for (var i = 0; i < count; i++)
        {
            bytes.Add(memory.Read((Word)(address + i)));
        }

        return bytes;
    }

    public static byte[] GetMemory(this IMemory memory)
    {
        switch (memory)
        {
            case Memory48K memory48:
                return memory48.Memory;

            case Memory16K memory16:
                return memory16.Memory;

            case Memory128K memory128:
                var memory64 = new byte[65536];
                var banks = memory128.ActiveBanks;

                Array.Copy(banks[0], 0, memory64, 0, 0x4000);
                Array.Copy(banks[1], 0, memory64, 0x4000, 0x4000);
                Array.Copy(banks[2], 0, memory64, 0x8000, 0x4000);
                Array.Copy(banks[3], 0, memory64, 0xC000, 0x4000);

                return memory64;
        }

        throw new NotSupportedException("Memory type not supported.");
    }

    public static Word ReadWord(this IMemory memory, Word address) =>
        (Word)(memory.Read(address) | memory.Read((Word)(address + 1)) << 8);
}
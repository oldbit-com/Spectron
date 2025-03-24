using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Extensions;

public static class MemoryExtensions
{
    public static List<byte> ReadBytes(this IMemory memory, Word address, int count)
    {
        var bytes = new List<byte>();

        for (var i = 0; i < count; i++)
        {
            bytes.Add(memory.Read((Word)(address + i)));
        }

        return bytes;
    }

    public static byte[] GetBytes(this IMemory memory)
    {
        var memory64 = new byte[65536];

        switch (memory)
        {
            case Memory48K memory48:
                memory48.Rom.CopyTo(memory64);
                memory48.Ram.CopyTo(memory64.AsSpan(0x4000));
                break;

            case Memory16K memory16:
                memory16.Rom.CopyTo(memory64);
                memory16.Ram.CopyTo(memory64.AsSpan(0x4000));
                break;

            case Memory128K memory128:
                var banks = memory128.ActiveBanks;

                Array.Copy(banks[0], 0, memory64, 0, 0x4000);
                Array.Copy(banks[1], 0, memory64, 0x4000, 0x4000);
                Array.Copy(banks[2], 0, memory64, 0x8000, 0x4000);
                Array.Copy(banks[3], 0, memory64, 0xC000, 0x4000);
                break;

            default:
                throw new NotSupportedException("Memory type not supported.");
        }

        return memory64;
    }

    public static Word ReadWord(this IMemory memory, Word address) =>
        (Word)(memory.Read(address) | memory.Read((Word)(address + 1)) << 8);
}
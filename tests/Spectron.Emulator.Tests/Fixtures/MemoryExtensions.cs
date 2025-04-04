using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulator.Tests.Fixtures;

internal static class MemoryExtensions
{
    internal static byte[] ReadAll(this IEmulatorMemory memory)
    {
        var result = new byte[65536];

        for (var address = 0; address < 65536; address++)
        {
            result[address] = memory.Read((Word)address);
        }

        return result;
    }

    internal static byte[] ReadRange(this IEmulatorMemory memory, int startAddress, int count)
    {
        var result = new byte[count];

        for (var i = 0; i < count; i++)
        {
            result[i] = memory.Read((Word)(startAddress + i));
        }

        return result;
    }

    internal static byte[] ReadScreen(this IEmulatorMemory memory)
    {
        var result = new byte[16384];

        for (Word address = 0; address < 16384; address++)
        {
            result[address] = memory.ReadScreen(address);
        }

        return result;
    }

    internal static void Fill(this IEmulatorMemory memory, int startAddress, int count, byte value)
    {
        for (var i = 0; i < count; i++)
        {
            memory.Write((Word)(startAddress + i), value);
        }
    }
}
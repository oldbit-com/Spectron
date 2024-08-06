using OldBit.Spectral.Emulation.Devices.Memory;

namespace OldBit.ZXSpectrum.Emulator.Tests.Fixtures;

public static class MemoryExtensions
{
    public static byte[] ReadAll(this EmulatorMemory memory)
    {
        var result = new byte[65536];

        for (var address = 0; address < 65536; address++)
        {
            result[address] = memory.Read((Word)address);
        }

        return result;
    }

    public static byte[] ReadRom(this EmulatorMemory memory)
    {
        var result = new byte[16384];

        for (var address = 0; address < 16384; address++)
        {
            result[address] = memory.Read((Word)address);
        }

        return result;
    }

    public static byte[] ReadRange(this EmulatorMemory memory, int startAddress, int count)
    {
        var result = new byte[count];

        for (var i = 0; i < count; i++)
        {
            result[i] = memory.Read((Word)(startAddress + i));
        }

        return result;
    }

    internal static byte[] ReadScreen(this EmulatorMemory memory)
    {
        var result = new byte[16384];

        for (Word address = 0; address < 16384; address++)
        {
            result[address] = memory.ReadScreen(address);
        }

        return result;
    }

    public static void Fill(this EmulatorMemory memory, int startAddress, int count, byte value)
    {
        for (var i = 0; i < count; i++)
        {
            memory.Write((Word)(startAddress + i), value);
        }
    }
}
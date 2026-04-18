using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulator.Tests.Fixtures;

internal static class MemoryExtensions
{
    extension(IEmulatorMemory memory)
    {
        internal byte[] ReadRange(int startAddress, int count)
        {
            var result = new byte[count];

            for (var i = 0; i < count; i++)
            {
                result[i] = memory.Read((Word)(startAddress + i));
            }

            return result;
        }

        internal byte[] ReadScreen()
        {
            var result = new byte[16384];

            for (Word address = 0; address < 16384; address++)
            {
                result[address] = memory.ReadScreen(address);
            }

            return result;
        }

        internal void Fill(int startAddress, int count, byte value)
        {
            for (var i = 0; i < count; i++)
            {
                memory.Write((Word)(startAddress + i), value);
            }
        }
    }
}
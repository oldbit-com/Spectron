using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Extensions;

public static class MemoryExtensions
{
    extension(IMemory memory)
    {
        public List<byte> ReadBytes(Word address, int count)
        {
            var bytes = new List<byte>();

            for (var i = 0; i < count; i++)
            {
                bytes.Add(memory.Read((Word)(address + i)));
            }

            return bytes;
        }

        public byte? Read(Word address, int? bank = null)
        {
            if (bank is < 8 && memory is Memory128K memory128)
            {
                return memory128.Banks[bank.Value][address - 0xC000];
            }

            return memory.Read(address);
        }

        public void Write(Word address, byte value, int? bank = null)
        {
            if (bank is < 8 && memory is Memory128K memory128)
            {
                memory128.Banks[bank.Value][address - 0xC000] = value;
            }
            else
            {
                memory.Write(address, value);
            }
        }

        public byte[] ToBytes()
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

        public Word ReadWord(Word address) =>
            (Word)(memory.Read(address) | memory.Read((Word)(address + 1)) << 8);
    }
}
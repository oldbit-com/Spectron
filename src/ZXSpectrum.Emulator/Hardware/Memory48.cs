using OldBit.Z80Cpu;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Memory48 : IMemory
{
    private byte[] Memory { get; } = new byte[65536];
    internal ReadOnlySpan<byte> Screen => new(Memory, 0x4000, 0x1C00);

    public Memory48(byte[] rom)
    {
        Array.Copy(rom, 0, Memory, 0, rom.Length);
    }

    public byte Read(Word address) => Memory[address];

    public void Write(Word address, byte data)
    {
        if (address >= 0x4000)
        {
            Memory[address] = data;
        }
    }
}
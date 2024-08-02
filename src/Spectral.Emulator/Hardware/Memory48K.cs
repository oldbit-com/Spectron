using OldBit.Spectral.Emulator.Rom;
using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulator.Hardware;

public class Memory48K : IMemory
{
    private byte[] Memory { get; } = new byte[65536];

    internal ReadOnlySpan<byte> Screen => new(Memory, 0x4000, 0x1C00);

    internal delegate void ScreenMemoryUpdatedEvent(Word address);

    internal event ScreenMemoryUpdatedEvent? ScreenMemoryUpdated;

    public Memory48K(byte[] rom) => Array.Copy(rom, 0, Memory, 0, rom.Length);

    public byte Read(Word address) => Memory[address];

    public void Write(Word address, byte data)
    {
        if (address < 0x4000)
        {
            return;
        }

        if (Memory[address] == data)
        {
            return;
        }

        Memory[address] = data;

        if (address < 0x5B00)
        {
            ScreenMemoryUpdated?.Invoke(address);
        }
    }
}
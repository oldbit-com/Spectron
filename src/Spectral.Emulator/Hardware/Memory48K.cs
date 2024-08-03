using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulator.Hardware;

/// <summary>
/// Memory for 48K Spectrum.
/// </summary>
internal class Memory48K : Memory
{
    private byte[] Memory { get; } = new byte[65536];

    public Memory48K(byte[] rom) => Array.Copy(rom, 0, Memory, 0, rom.Length);

    public override byte Read(Word address) => Memory[address];

    public override void Write(Word address, byte data)
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
            OnScreenMemoryUpdated(address);
        }
    }

    internal override ReadOnlySpan<byte> Screen => new(Memory, 0x4000, 0x1C00);
}
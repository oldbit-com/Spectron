using OldBit.Z80Cpu;
using OldBit.ZXSpectrum.Emulator.Rom;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

internal class ScreenMemoryUpdatedEventArgs(Word address)
{
    public Word Address { get; } = address;
}

public class Memory48K : IMemory
{
    private byte[] Memory { get; } = new byte[65536];

    internal ReadOnlySpan<byte> Screen => new(Memory, 0x4000, 0x1C00);

    internal delegate void ScreenMemoryUpdatedEventHandler(ScreenMemoryUpdatedEventArgs e);

    internal event ScreenMemoryUpdatedEventHandler? ScreenMemoryUpdated;

    public Memory48K()
    {
        var rom = RomReader.Read48Rom();

        Array.Copy(rom, 0, Memory, 0, rom.Length);
    }

    public byte Read(Word address) => Memory[address];

    public void Write(Word address, byte data)
    {
        if (address < 0x4000)
        {
            return;
        }

        Memory[address] = data;

        if (address < 0x5B00)
        {
            ScreenMemoryUpdated?.Invoke(new ScreenMemoryUpdatedEventArgs(address));
        }
    }
}
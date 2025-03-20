namespace OldBit.Spectron.Emulation.Devices.Memory;

public class RomMemory(byte[] rom) : IRomMemory
{
    public byte[] Memory { get; } = rom;

    public byte Read(Word address) => Memory[address];

    public void Write(Word address, byte data) { }
}
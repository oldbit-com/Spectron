namespace OldBit.Spectron.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 48k.
/// </summary>
internal sealed class Memory48K : IEmulatorMemory
{
    internal Memory48K(byte[] rom) => Array.Copy(rom, 0, Memory, 0, rom.Length);

    internal byte[] Memory { get; } = new byte[65536];

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

    public event ScreenMemoryUpdatedEvent? ScreenMemoryUpdated;
}
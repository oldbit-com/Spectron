namespace OldBit.Spectron.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 16k.
/// </summary>
internal sealed class Memory16K : IEmulatorMemory
{
    internal Memory16K(byte[] rom) => Array.Copy(rom, 0, Memory, 0, rom.Length);

    internal byte[] Memory { get; } = new byte[32768];

    public byte Read(Word address) => address > 32767 ? (byte)0xFF : Memory[address];

    public void Write(Word address, byte data)
    {
        if (address is < 0x4000 or > 32767)
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
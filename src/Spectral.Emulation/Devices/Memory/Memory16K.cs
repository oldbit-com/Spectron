namespace OldBit.Spectral.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 16k.
/// </summary>
internal sealed class Memory16K : EmulatorMemory
{
    private byte[] Memory { get; } = new byte[32768];

    internal Memory16K(byte[] rom) => Array.Copy(rom, 0, Memory, 0, rom.Length);

    public override byte Read(Word address) => address > 32767 ? (byte)0xFF : Memory[address];

    public override void Write(Word address, byte data)
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
            OnScreenMemoryUpdated(address);
        }
    }
}
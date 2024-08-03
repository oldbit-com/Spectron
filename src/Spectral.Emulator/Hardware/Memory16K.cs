namespace OldBit.Spectral.Emulator.Hardware;

/// <summary>
/// Memory for 16K Spectrum.
/// </summary>
internal class Memory16K : Memory
{
    private byte[] Memory { get; } = new byte[32768];

    public Memory16K(byte[] rom) => Array.Copy(rom, 0, Memory, 0, rom.Length);

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

    internal override ReadOnlySpan<byte> Screen => new(Memory, 0x4000, 0x1C00);
}
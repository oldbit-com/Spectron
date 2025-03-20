namespace OldBit.Spectron.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 16k.
/// </summary>
internal sealed class Memory16K : IEmulatorMemory
{
    private readonly IRomMemory _originalRom;
    private IRomMemory _activeRom;

    internal Memory16K(byte[] rom)
    {
        _originalRom = new RomMemory(rom);
        _activeRom = _originalRom;

        Array.Copy(rom, 0, Memory, 0, rom.Length);
        Array.Fill(Memory, (byte)0xFF, 32768, 32768);
    }

    internal byte[] Memory { get; } = new byte[65536];

    public byte Read(Word address) =>
        address < 0x4000 ? _activeRom.Read(address) : Memory[address];

    public void Write(Word address, byte data)
    {
        switch (address)
        {
            case < 0x4000:
                _activeRom.Write(address, data);
                return;

            case > 32767:
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

    public void ShadowRom(IRomMemory? shadowRom) => _activeRom = shadowRom ?? _originalRom;

    public event ScreenMemoryUpdatedEvent? ScreenMemoryUpdated;
}
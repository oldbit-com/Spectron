namespace OldBit.Spectron.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 48k.
/// </summary>
internal sealed class Memory48K : IEmulatorMemory
{
    private readonly IRomMemory _originalRom;
    private IRomMemory _activeRom;

    internal Memory48K(byte[] rom)
    {
        _originalRom = new RomMemory(rom);
        _activeRom = _originalRom;

        Array.Copy(rom, 0, Memory, 0, rom.Length);
    }

    internal byte[] Memory { get; } = new byte[65536];

    public byte Read(Word address) =>
        address < 0x4000 ? _activeRom.Read(address) : Memory[address];

    public void Write(Word address, byte data)
    {
        if (address < 0x4000)
        {
            _activeRom.Write(address, data);

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
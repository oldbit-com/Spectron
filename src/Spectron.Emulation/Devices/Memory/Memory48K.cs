namespace OldBit.Spectron.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 48k.
/// </summary>
internal sealed class Memory48K : IEmulatorMemory
{
    private readonly byte[] _memory = new byte[65536];
    private IRomMemory _activeRom;

    public IRomMemory OriginalRom { get; }

    internal Memory48K(byte[] rom)
    {
        OriginalRom = new RomMemory(rom);
        _activeRom = OriginalRom;
    }

    internal Span<byte> Ram => _memory.AsSpan()[0x4000..];

    internal ReadOnlySpan<byte> Rom => _activeRom.Memory;

    public byte Read(Word address) =>
        address < 0x4000 ? _activeRom.Read(address) : _memory[address];

    public void Write(Word address, byte data)
    {
        if (address < 0x4000)
        {
            _activeRom.Write(address, data);

            return;
        }

        if (_memory[address] == data)
        {
            return;
        }

        _memory[address] = data;

        MemoryUpdated?.Invoke(address, data);
    }

    public void ShadowRom(IRomMemory? shadowRom) => _activeRom = shadowRom ?? OriginalRom;

    public event MemoryUpdatedEvent? MemoryUpdated;
}
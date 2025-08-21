namespace OldBit.Spectron.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 16k.
/// </summary>
internal sealed class Memory16K : IEmulatorMemory
{
    private readonly byte[] _memory = new byte[65536];
    private IRomMemory _activeRom;

    public IRomMemory OriginalRom { get; }

    internal Memory16K(byte[] rom)
    {
        OriginalRom = new RomMemory(rom);
        _activeRom = OriginalRom;

        Array.Fill(_memory, (byte)0xFF, 32768, 32768);
    }

    internal Span<byte> Ram => _memory.AsSpan()[0x4000..0x8000];

    internal ReadOnlySpan<byte> Rom => _activeRom.Memory;

    public byte Read(Word address) =>
        address < 0x4000 ? _activeRom.Read(address) : _memory[address];

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

        if (_memory[address] == data)
        {
            return;
        }

        _memory[address] = data;

        MemoryUpdated?.Invoke(address);
    }

    public void ShadowRom(IRomMemory? shadowRom) => _activeRom = shadowRom ?? OriginalRom;

    public event MemoryUpdatedEvent? MemoryUpdated;
}
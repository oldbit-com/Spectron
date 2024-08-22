namespace OldBit.Spectron.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 48k.
/// </summary>
internal sealed class Memory48K : IEmulatorMemory
{
    private readonly byte[] _memory = new byte[65536];

    internal Memory48K(byte[] rom) => Array.Copy(rom, 0, _memory, 0, rom.Length);

    public byte Read(Word address) => _memory[address];

    public void Write(Word address, byte data)
    {
        if (address < 0x4000)
        {
            return;
        }

        if (_memory[address] == data)
        {
            return;
        }

        _memory[address] = data;

        if (address < 0x5B00)
        {
            ScreenMemoryUpdated?.Invoke(address);
        }
    }

    public event ScreenMemoryUpdatedEvent? ScreenMemoryUpdated;
}
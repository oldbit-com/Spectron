namespace OldBit.Spectron.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 16k.
/// </summary>
internal sealed class Memory16K : IEmulatorMemory
{
    private readonly byte[] _memory = new byte[32768];

    internal Memory16K(byte[] rom) => Array.Copy(rom, 0, _memory, 0, rom.Length);

    public byte Read(Word address) => address > 32767 ? (byte)0xFF : _memory[address];

    public void Write(Word address, byte data)
    {
        if (address is < 0x4000 or > 32767)
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
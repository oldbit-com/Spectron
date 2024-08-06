namespace OldBit.Spectral.Emulation.Devices.Memory;

/// <summary>
/// Memory for the Spectrum 48k.
/// </summary>
internal sealed class Memory48K : EmulatorMemory
{
    private readonly byte[] _memory = new byte[65536];

    internal Memory48K(byte[] rom)
    {
        if (rom.Length != 16384)
        {
            throw new ArgumentException("ROM must be exactly 16KB in size.", nameof(rom));
        }

        Array.Copy(rom, 0, _memory, 0, rom.Length);
    }

    public override byte Read(Word address) => _memory[address];

    public override void Write(Word address, byte data)
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
            OnScreenMemoryUpdated(address);
        }
    }
}
using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulator.Hardware;

internal abstract class Memory : IMemory
{
    public abstract byte Read(Word address);

    public abstract void Write(Word address, byte data);

    internal abstract ReadOnlySpan<byte> Screen { get; }

    internal delegate void ScreenMemoryUpdatedEvent(Word address);
    internal event ScreenMemoryUpdatedEvent? ScreenMemoryUpdated;

    protected void OnScreenMemoryUpdated(Word address) => ScreenMemoryUpdated?.Invoke(address);
}
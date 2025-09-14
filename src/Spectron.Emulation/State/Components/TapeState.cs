using MemoryPack;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record TapeState
{
    public int CurrentBlockNo { get; set; }

    public byte[] TzxData { get; set; } = [];
}
using MemoryPack;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record TimexState
{
    public byte PortFF { get; set; }
}
using MemoryPack;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record AyState
{
    public byte CurrentRegister { get; set; }

    public byte[] Registers { get; set; } = [];
}
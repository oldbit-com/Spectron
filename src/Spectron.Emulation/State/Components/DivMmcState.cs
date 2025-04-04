using MemoryPack;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public partial record DivMmcState
{
    public byte ControlRegister { get; set; }

    public byte[][] Banks { get; set; } = [];
}
using MemoryPack;
using OldBit.Spectron.Emulation.Extensions;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record CustomRomState
{
    public RomType RomType { get; set; }

    public byte[]? Bank0 { get; set; } = [];

    public byte[]? Bank1 { get; set; } = [];

    [MemoryPackIgnore]
    internal byte[]? Concatenated => Bank0.Concatenate(Bank1);
}
using MemoryPack;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record UlaPlusState
{
    public bool IsActive { get; set; }

    public bool IsEnabled { get; set; }

    public byte PaletteGroup { get; set; }

    public Color[][] PaletteColors { get; set; } = [];
}
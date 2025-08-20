using MemoryPack;
using OldBit.Spectron.Emulation.Devices.Interface1;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public partial record Interface1State
{
    public RomVersion RomVersion { get; init; }
}
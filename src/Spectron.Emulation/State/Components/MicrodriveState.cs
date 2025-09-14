using MemoryPack;
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public partial class MicrodriveState
{
    public MicrodriveId MicrodriveId { get; init; }

    public string? FilePath { get; set; }

    public byte[] Data { get; init; } = [];
}
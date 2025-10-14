using MemoryPack;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public partial record Beta128State
{
    public byte ControlRegister { get; init; }

    public byte CommandRegister { get; init; }

    public byte DataRegister { get; init; }

    public byte TrackRegister { get; init; }

    public byte SectorRegister { get; init; }

    public bool IsRomPaged { get; init; }

    public DiskDriveState[] Drives { get; init; } = [];
}
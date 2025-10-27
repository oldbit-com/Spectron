using MemoryPack;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Beta128.Image;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public partial record DiskDriveState
{
    public DriveId DriveId { get; init; }

    public string? FilePath { get; set; }

    public DiskImageType DiskImageType { get; init; }

    public bool IsWriteProtected { get; init; }

    public byte[] Data { get; init; } = [];
}
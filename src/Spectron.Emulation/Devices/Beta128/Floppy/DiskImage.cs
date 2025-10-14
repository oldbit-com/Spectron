using OldBit.Spectron.Emulation.Devices.Beta128.Image;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

public sealed class DiskImage
{
    internal FloppyDisk Floppy { get; }
    internal DiskImageType DiskImageType { get; }

    public string? FilePath { get; private set; }

    internal DiskImage(string filePath)
    {
        FilePath = filePath;
        DiskImageType = DiskImageType.Trd;

        Floppy = TrdDisk.Read(filePath);
    }

    public byte[] GetData()
    {
        return [];
    }
}
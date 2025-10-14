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

        Floppy = DiskReader.Read(filePath);
    }

    internal DiskImage(string? filePath, DiskImageType diskImageType, ReadOnlySpan<byte> data)
    {
        FilePath = filePath;
        DiskImageType = diskImageType;

        Floppy = DiskReader.Read(diskImageType, data);
    }

    public byte[] GetData()
    {
        using var stream = new MemoryStream();

        DiskWriter.Write(this, stream);

        return stream.ToArray();
    }
}
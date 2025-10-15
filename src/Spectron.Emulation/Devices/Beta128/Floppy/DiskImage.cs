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

        Floppy = DiskImageReader.Read(filePath);
    }

    internal DiskImage(string? filePath, DiskImageType diskImageType, ReadOnlySpan<byte> data)
    {
        FilePath = filePath;
        DiskImageType = diskImageType;

        Floppy = DiskImageReader.Read(diskImageType, data);
    }

    public byte[] GetData()
    {
        using var stream = new MemoryStream();

        DiskImageWriter.Write(this, stream);

        return stream.ToArray();
    }
}
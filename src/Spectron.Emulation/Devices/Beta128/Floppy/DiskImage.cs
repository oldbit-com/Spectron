using OldBit.Spectron.Emulation.Devices.Beta128.Image;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

/// <summary>
/// Represents a disk image for a floppy disk, allowing for reading and writing of the image data.
/// The disk image can be created from a file path or from raw data, and supports specific disk image types.
/// </summary>
public sealed class DiskImage
{
    internal FloppyDisk Floppy { get; }
    internal DiskImageType DiskImageType { get; }

    public string? FilePath { get; private set; }

    internal DiskImage(string filePath)
    {
        FilePath = filePath;
        DiskImageType = DiskImageReader.GetImageType(filePath);
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
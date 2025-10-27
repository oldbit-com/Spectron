using OldBit.Spectron.Emulation.Devices.Beta128.Image;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

/// <summary>
/// Represents a disk image for a floppy disk, allowing for reading and writing of the image data.
/// The disk image can be created from a file path or from raw data, and supports specific disk image types.
/// </summary>
public sealed class DiskFile
{
    internal DiskImageType ImageType { get; }

    public FloppyDisk Floppy { get; }
    public string? FilePath { get; private set; }

    internal DiskFile()
    {
        ImageType = DiskImageType.Trd;
        Floppy = new FloppyDisk(80, 2, "New Disk");
    }

    internal DiskFile(string filePath)
    {
        FilePath = filePath;
        ImageType = DiskImage.GetImageType(filePath);
        Floppy = DiskImage.Read(filePath);
    }

    internal DiskFile(string? filePath, DiskImageType imageType, ReadOnlySpan<byte> data)
    {
        FilePath = filePath;
        ImageType = imageType;

        Floppy = DiskImage.Read(imageType, data);
    }

    public byte[] GetData()
    {
        using var stream = new MemoryStream();

        DiskImage.Write(ImageType, Floppy, stream);

        return stream.ToArray();
    }

    public async Task SaveAsync(string filePath)
    {
        await using var file = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        var imageType = DiskImage.GetImageType(filePath);

        if (imageType == DiskImageType.Unknown)
        {
            imageType = ImageType;
        }

        DiskImage.Write(imageType, Floppy, file);

        FilePath = filePath;

        file.Flush();
        file.Close();
    }
}
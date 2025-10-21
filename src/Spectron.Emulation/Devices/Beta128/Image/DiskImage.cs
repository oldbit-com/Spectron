using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Image;

internal static class DiskImage
{
    internal static FloppyDisk Read(string filePath)
    {
        var diskImageType = GetImageType(filePath);

        IDiskImage image = diskImageType switch
        {
            DiskImageType.Trd => new TrdDiskImage(),
            DiskImageType.Scl => new SclDiskImage(),
            _ => throw new NotSupportedException($"Unsupported disk format: {diskImageType}")
        };

        return image.Read(filePath);
    }

    internal static FloppyDisk Read(DiskImageType imageType, ReadOnlySpan<byte> data)
    {
        IDiskImage image = imageType switch
        {
            DiskImageType.Trd => new TrdDiskImage(),
            DiskImageType.Scl => new SclDiskImage(),
            _ => throw new NotSupportedException($"Unsupported disk format: {imageType}")
        };

        return image.Read(data);
    }

    internal static void Write(DiskImageType imageType, FloppyDisk floppyDisk, Stream stream)
    {
        IDiskImage image = imageType switch
        {
            DiskImageType.Trd => new TrdDiskImage(),
            DiskImageType.Scl => new SclDiskImage(),
            _ => throw new NotSupportedException($"Unsupported disk format: {imageType}")
        };

        image.Write(floppyDisk, stream);
    }

    internal static DiskImageType GetImageType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".trd" => DiskImageType.Trd,
            ".scl" => DiskImageType.Scl,
            _ => DiskImageType.Unknown,
        };
    }
}
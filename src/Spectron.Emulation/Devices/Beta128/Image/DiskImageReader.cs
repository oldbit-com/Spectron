using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Image;

internal static class DiskImageReader
{
    internal static FloppyDisk Read(string filePath)
    {
        var diskImageType = GetImageType(filePath);

        return diskImageType switch
        {
            DiskImageType.Trd => TrdDiskImage.Read(filePath),
            DiskImageType.Scl => SclDiskImage.Read(filePath),
            _ => throw new NotSupportedException($"Unsupported disk format: {diskImageType}")
        };
    }

    internal static FloppyDisk Read(DiskImageType diskImageType, ReadOnlySpan<byte> data)
    {
        return diskImageType switch
        {
            DiskImageType.Trd => TrdDiskImage.Read(data),
            DiskImageType.Scl => SclDiskImage.Read(data),
            _ => throw new NotSupportedException($"Unsupported disk format: {diskImageType}")
        };
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
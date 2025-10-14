using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Image;

internal static class DiskReader
{
    internal static FloppyDisk Read(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".trd" => TrdDiskImage.Read(filePath),
            _ => throw new NotSupportedException($"Unsupported disk format: {extension}")
        };
    }

    internal static FloppyDisk Read(DiskImageType diskImageType, ReadOnlySpan<byte> data)
    {
        return diskImageType switch
        {
            DiskImageType.Trd => TrdDiskImage.Read(data),
            _ => throw new NotSupportedException($"Unsupported disk format: {diskImageType}")
        };
    }
}
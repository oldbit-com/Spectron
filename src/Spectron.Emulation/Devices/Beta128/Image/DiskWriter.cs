using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Image;

internal static class DiskWriter
{
    internal static void Write(DiskImage image, Stream stream)
    {
        switch (image.DiskImageType)
        {
            case DiskImageType.Trd:
                TrdDiskImage.Write(image.Floppy, stream);
                break;

            default:
                throw new NotSupportedException($"Unsupported disk format: {image.DiskImageType}");
        }
    }
}
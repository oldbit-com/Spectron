using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Image;

internal interface IDiskImage
{
    FloppyDisk Read(string filePath);

    FloppyDisk Read(ReadOnlySpan<byte> data);

    void Write(FloppyDisk disk, Stream stream);
}
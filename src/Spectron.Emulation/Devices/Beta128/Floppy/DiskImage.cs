namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

public sealed class DiskImage
{
    internal FloppyDisk Floppy { get; }

    public string? FilePath { get; private set; }

    internal DiskImage(string filePath)
    {
        FilePath = filePath;

        Floppy = TrdDisk.Read(filePath);
    }
}
namespace OldBit.Spectral.Emulation.File;

public enum FileType
{
    Tap,

    Tzx,

    Sna,

    Z80,

    Szx,

    Unsupported
}

public static class FileTypeExtensions
{
    public static bool IsSnapshot(this FileType fileType) =>
        fileType is FileType.Sna or FileType.Z80 or FileType.Szx;
}
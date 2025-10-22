namespace OldBit.Spectron.Emulation.Files;

public enum FileType
{
    Tap,

    Tzx,

    Sna,

    Z80,

    Szx,

    Zip,

    Pok,

    Mdr,

    Trd,

    Scl,

    Unsupported
}

public static class FileTypeExtensions
{
    public static bool IsSnapshot(this FileType fileType) =>
        fileType is FileType.Sna or FileType.Z80 or FileType.Szx;

    public static bool IsTape(this FileType fileType) =>
        fileType is FileType.Tap or FileType.Tzx;

    public static bool IsArchive(this FileType fileType) =>
        fileType is FileType.Zip;

    public static bool IsMicrodrive(this FileType fileType) =>
        fileType is FileType.Mdr;
}
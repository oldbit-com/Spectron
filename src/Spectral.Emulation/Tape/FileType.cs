namespace OldBit.Spectral.Emulation.Tape;

internal enum FileType
{
    Tap,

    Tzx,

    Sna,

    Z80,

    Szx,

    Unsupported
}

internal static class FileTypeExtensions
{
    internal static bool IsSnapshot(this FileType fileType) =>
        fileType is FileType.Sna or FileType.Z80 or FileType.Szx;
}
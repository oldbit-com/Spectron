using OldBit.Spectron.Emulation.Files;

namespace OldBit.Spectron.Emulation.Extensions;

public static class FileTypeExtensions
{
    extension(FileType fileType)
    {
        public bool IsSnapshot() =>
            fileType is FileType.Sna or FileType.Z80 or FileType.Szx or FileType.Spectron;

        public bool IsTape() =>
            fileType is FileType.Tap or FileType.Tzx;

        public bool IsArchive() =>
            fileType is FileType.Zip;

        public bool IsMicrodrive() =>
            fileType is FileType.Mdr;
    }
}
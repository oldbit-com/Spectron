namespace OldBit.Spectron.Emulation.Storage;

public static class FileTypeHelper
{
    public static FileType GetFileType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".sna" => FileType.Sna,
        ".szx" => FileType.Szx,
        ".z80" => FileType.Z80,
        ".tap" => FileType.Tap,
        ".tzx" => FileType.Tzx,
        _ => FileType.Unsupported
    };
}
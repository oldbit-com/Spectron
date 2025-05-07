namespace OldBit.Spectron.Emulation.Files;

public static class FileTypeHelper
{
    public static FileType GetFileType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".sna" => FileType.Sna,
        ".szx" => FileType.Szx,
        ".z80" => FileType.Z80,
        ".tap" => FileType.Tap,
        ".tzx" => FileType.Tzx,
        ".zip" => FileType.Zip,
        ".pok" => FileType.Pok,
        _ => FileType.Unsupported
    };
}
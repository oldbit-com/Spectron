namespace OldBit.Spectron.Emulation.Files;

public static class FileTypes
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
        ".mdr" => FileType.Mdr,
        ".trd" => FileType.Trd,
        ".scl" => FileType.Scl,
        ".spectron" => FileType.Spectron,
        _ => FileType.Unsupported
    };
}
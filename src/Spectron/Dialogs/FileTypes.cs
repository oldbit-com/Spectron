using Avalonia.Platform.Storage;

namespace OldBit.Spectron.Dialogs;

public static class FileTypes
{
    public static FilePickerFileType All { get; } = new("All Files")
    {
        Patterns = ["*.tap", "*.tzx", "*.sna", "*.szx", "*.z80", "*.zip"]
    };

    public static FilePickerFileType TapeFiles { get; } = new("Tape Files")
    {
        Patterns = ["*.tap", "*.tzx"]
    };

    public static FilePickerFileType Tap { get; } = new("TAP File")
    {
        Patterns = ["*.tap"]
    };

    public static FilePickerFileType Tzx { get; } = new("TZX File")
    {
        Patterns = ["*.tzx"]
    };

    public static FilePickerFileType Sna { get; } = new("SNA File")
    {
        Patterns = ["*.sna"]
    };

    public static FilePickerFileType Szx { get; } = new("SZX File")
    {
        Patterns = ["*.szx"]
    };

    public static FilePickerFileType Z80 { get; } = new("Z80 File")
    {
        Patterns = ["*.z80"]
    };

    public static FilePickerFileType Zip { get; } = new("ZIP File")
    {
        Patterns = ["*.zip"]
    };
}
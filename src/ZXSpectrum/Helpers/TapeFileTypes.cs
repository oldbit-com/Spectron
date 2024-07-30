using Avalonia.Platform.Storage;

namespace OldBit.ZXSpectrum.Helpers;

public static class TapeFileTypes
{
    public static FilePickerFileType All { get; } = new("All Files")
    {
        Patterns = new[] { "*.tap", "*.tzx", "*.sna", "*.szx", "*.z80" }
    };

    public static FilePickerFileType Tap { get; } = new("TAP File")
    {
        Patterns = new[] { "*.tap" }
    };

    public static FilePickerFileType Tzx { get; } = new("TZX File")
    {
        Patterns = new[] { "*.tzx" },
    };

    public static FilePickerFileType Sna { get; } = new("SNA File")
    {
        Patterns = new[] { "*.sna" }
    };

    public static FilePickerFileType Szx { get; } = new("SZX File")
    {
        Patterns = new[] { "*.szx" }
    };

    public static FilePickerFileType Z80 { get; } = new("Z80 File")
    {
        Patterns = new[] { "*.z80" },
    };
}
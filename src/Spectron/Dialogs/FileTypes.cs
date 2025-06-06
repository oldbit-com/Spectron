using Avalonia.Platform.Storage;

namespace OldBit.Spectron.Dialogs;

public static class FileTypes
{
    public static FilePickerFileType All { get; } = new("All Files")
    {
        Patterns = ["*.tap", "*.tzx", "*.sna", "*.szx", "*.z80", "*.zip", "*.pok"]
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

    public static FilePickerFileType Wav { get; } = new("Wave Audio")
    {
        Patterns = ["*.wav"]
    };

    public static FilePickerFileType Mp4 { get; } = new("MP4 Video")
    {
        Patterns = ["*.mp4"]
    };

    public static FilePickerFileType Png { get; } = new("PNG Bitmap")
    {
        Patterns = ["*.png"]
    };

    public static FilePickerFileType Bin { get; } = new("BIN File")
    {
        Patterns = ["*.bin"]
    };

    public static FilePickerFileType Rom { get; } = new("ROM File")
    {
        Patterns = ["*.rom"]
    };

    public static FilePickerFileType Img { get; } = new("Disk Image")
    {
        Patterns = ["*.img"]
    };

    public static FilePickerFileType Pok { get; } = new("POKE File")
    {
        Patterns = ["*.pok"]
    };

    public static FilePickerFileType Any { get; } = new("All Files")
    {
        Patterns = ["*.*"]
    };
}
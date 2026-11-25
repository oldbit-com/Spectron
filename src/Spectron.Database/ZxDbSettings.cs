namespace OldBit.Spectron.Database;

public sealed class ZxDbSettings
{
    public string? DatabasePath { get; set; }

    public string DownloadDirectory { get; init; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OldBit", "Spectron", "ZxDb");

    public bool IsAvailable => !string.IsNullOrWhiteSpace(DatabasePath) && File.Exists(DatabasePath);
}

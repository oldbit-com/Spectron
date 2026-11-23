namespace OldBit.Spectron.Database;

public sealed class ZxDbSettings
{
    public string? DatabasePath { get; set; }

    public bool IsAvailable => !string.IsNullOrWhiteSpace(DatabasePath) && File.Exists(DatabasePath);
}

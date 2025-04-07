namespace OldBit.Spectron.Settings;

public record DivMmcSettings
{
    public bool IsEnabled { get; init; }

    public bool IsEepromWriteEnabled { get; init; }

    public string Card0FileName { get; init; } = string.Empty;

    public string Card1FileName { get; init; } = string.Empty;

    public bool IsDriveWriteEnabled { get; init; }
}
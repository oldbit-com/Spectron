namespace OldBit.Spectron.Settings;

public record DivMmcSettings
{
    public bool IsEnabled { get; init; }

    public bool IsWriteEnabled { get; init; }

    public string Card0FileName { get; init; } = string.Empty;
}
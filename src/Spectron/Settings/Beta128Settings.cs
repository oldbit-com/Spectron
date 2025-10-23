namespace OldBit.Spectron.Settings;

public record Beta128Settings
{
    public bool IsEnabled { get; init; }

    public int NumberOfDrives { get; init; } = 2;
}
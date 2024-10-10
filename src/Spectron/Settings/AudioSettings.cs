namespace OldBit.Spectron.Settings;

public record AudioSettings
{
    public bool IsBeeperEnabled { get; init; } = true;

    public bool IsAyAudioEnabled { get; init; } = true;

    public bool IsAyAudioEnabled48K { get; init; } = true;
}
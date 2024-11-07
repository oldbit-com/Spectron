namespace OldBit.Spectron.Settings;

public record AudioSettings
{
    public bool IsMuted { get; set; }

    public bool IsBeeperEnabled { get; init; } = true;

    public bool IsAyAudioEnabled { get; init; } = true;

    public bool IsAySupportedStandardSpectrum { get; init; } = true;
}
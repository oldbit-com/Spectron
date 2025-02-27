using OldBit.Spectron.Screen;

namespace OldBit.Spectron.Settings;

public record RecordingSettings
{
    public BorderSize BorderSize { get; init; } = BorderSize.Medium;

    public int ScalingFactor { get; init; } = 2;

    public string ScalingAlgorithm { get; init; } = "neighbor";

    public string FFmpegPath { get; init; } = string.Empty;
}
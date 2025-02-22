namespace OldBit.Spectron.Recorder;

public record RecorderOptions
{
    public int BorderLeft { get; init; }
    public int BorderTop { get; init; }
    public int BorderRight { get; init; }
    public int BorderBottom { get; init; }

    public int AudioChannels { get; init; }

    public int ScalingFactor { get; init; } = 2;
    public string ScalingAlgorithm { get; init; } = "neighbor";
}
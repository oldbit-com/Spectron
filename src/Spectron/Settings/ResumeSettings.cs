namespace OldBit.Spectron.Settings;

public record ResumeSettings
{
    public bool IsResumeEnabled { get; init; } = true;

    public bool ShouldIncludeTape { get; init; } = true;

    public bool ShouldIncludeTimeMachine { get; init; } = true;
}
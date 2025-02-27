using System;

namespace OldBit.Spectron.Settings;

public record TimeMachineSettings
{
    public bool IsEnabled { get; init; } = true;

    public TimeSpan SnapshotInterval { get; init; } = TimeSpan.FromSeconds(1);

    public TimeSpan MaxDuration { get; init; } = TimeSpan.FromMinutes(1);

    public int CountdownSeconds { get; init; } = 3;
}
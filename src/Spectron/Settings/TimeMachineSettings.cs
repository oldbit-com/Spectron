using System;

namespace OldBit.Spectron.Settings;

public record TimeMachineSettings
{
    public bool IsEnabled { get; set; } = true;

    public TimeSpan SnapshotInterval { get; set; } = TimeSpan.FromSeconds(1);

    public TimeSpan MaxDuration { get; set; } = TimeSpan.FromMinutes(1);
}
using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Settings;

public record TimeMachineSnapshot(string Snapshot, DateTimeOffset Timestamp);

public record SessionSettings
{
    public string? LastSnapshot { get; set; }

    public List<TimeMachineSnapshot> TimeMachineSnapshots { get; } = [];
}
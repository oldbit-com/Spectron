using System.Collections.Generic;

namespace OldBit.Spectron.Settings;

public class SessionSettings
{
    public string? LastSnapshot { get; set; }

    public List<string> TimeMachineSnapshots { get; } = [];
}
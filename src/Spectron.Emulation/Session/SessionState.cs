namespace OldBit.Spectron.Emulation.Session;

internal record SessionState
{
    internal string? LastSnapshot { get; set; }

    public List<string> TimeMachineSnapshots { get; } = [];
}

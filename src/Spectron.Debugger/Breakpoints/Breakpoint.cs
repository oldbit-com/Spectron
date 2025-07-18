namespace OldBit.Spectron.Debugger.Breakpoints;

public abstract class Breakpoint
{
    public bool IsEnabled { get; set; } = true;

    public int Value { get; protected init; }

    public int? ValueAtLastHit { get; set; }

    public string Condition { get; init; } = string.Empty;
}
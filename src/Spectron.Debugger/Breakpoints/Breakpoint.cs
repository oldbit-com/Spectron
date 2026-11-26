namespace OldBit.Spectron.Debugger.Breakpoints;

public abstract class Breakpoint
{
    public int Id { get; internal set; }

    public bool IsEnabled { get; set; } = true;

    public string Condition { get; init; } = string.Empty;
}

namespace OldBit.Spectron.Debugger.Breakpoints;

public abstract class Breakpoint
{
    public bool IsEnabled { get; set; } = true;

    public string Condition { get; init; } = string.Empty;
}
namespace OldBit.Spectron.Debugger.Breakpoints;

public abstract class Breakpoint
{
    /// <summary>
    /// Gets the identifier assigned by <see cref="BreakpointManager"/> when the breakpoint is added.
    /// It is kept when the breakpoint is updated and never reused.
    /// </summary>
    public int Id { get; internal set; }

    public bool IsEnabled { get; set; } = true;

    public string Condition { get; init; } = string.Empty;
}

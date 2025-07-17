namespace OldBit.Spectron.Debugger.Breakpoints;

public abstract class Breakpoint
{
    public Guid Id { get; } = Guid.NewGuid();

    public bool IsEnabled { get; set; } = true;

    public int Value { get; set; }

    public int? ValueAtLastHit { get; set; }
}
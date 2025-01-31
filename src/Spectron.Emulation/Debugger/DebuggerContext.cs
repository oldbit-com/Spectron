namespace OldBit.Spectron.Emulation.Debugger;

public sealed class DebuggerContext
{
    private readonly HashSet<Word> _breakpoints = [];

    public IReadOnlySet<Word> Breakpoints => _breakpoints;

    public DebuggerContext()
    {
    }

    public void ClearBreakpoints()
    {
        _breakpoints.Clear();
    }

    public void AddBreakpoint(Word address) => _breakpoints.Add(address);

    public void RemoveBreakpoint(Word address) => _breakpoints.Remove(address);

    public bool HasBreakpoint(Word address) => _breakpoints.Contains(address);
}
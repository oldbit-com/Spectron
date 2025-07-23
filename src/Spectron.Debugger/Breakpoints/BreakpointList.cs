namespace OldBit.Spectron.Debugger.Breakpoints;

public sealed class BreakpointList
{
    private readonly List<Breakpoint> _breakpoints = [];

    public IReadOnlyList<Breakpoint> Breakpoints => _breakpoints;
    public IReadOnlyList<RegisterBreakpoint> Register { get; private set; } = [];
    public IReadOnlyList<MemoryBreakpoint> Memory { get; private set; } = [];

    public void Add(Breakpoint breakpoint)
    {
        _breakpoints.Add(breakpoint);

        SynchronizeLists();
    }

    public void AddIfNotExists(Breakpoint breakpoint)
    {
        if (!_breakpoints.Contains(breakpoint))
        {
            Add(breakpoint);
        }
    }

    public void Remove(Breakpoint breakpoint)
    {
        _breakpoints.Remove(breakpoint);

        SynchronizeLists();
    }

    public void Replace(Breakpoint original, Breakpoint replacement)
    {
        var existingIndex = _breakpoints.IndexOf(original);

        if (existingIndex < 0)
        {
            return;
        }

        _breakpoints[existingIndex] = replacement;

        SynchronizeLists();
    }

    private void SynchronizeLists()
    {
        Register = _breakpoints.Where(x => x is RegisterBreakpoint).Cast<RegisterBreakpoint>().ToList();
        Memory = _breakpoints.Where(x => x is MemoryBreakpoint).Cast<MemoryBreakpoint>().ToList();
    }
}
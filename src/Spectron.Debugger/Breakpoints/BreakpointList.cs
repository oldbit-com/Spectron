namespace OldBit.Spectron.Debugger.Breakpoints;

/// <summary>
/// Breakpoints are changed from the UI and other threads while the emulation thread checks them,
/// so changes are made under a lock and the lists are replaced, never modified. Readers take
/// a reference to a list once and iterate it without locking.
/// </summary>
public sealed class BreakpointList
{
    private readonly Lock _lock = new();
    private readonly List<Breakpoint> _breakpoints = [];

    public IReadOnlyList<Breakpoint> Breakpoints { get; private set; } = [];
    public IReadOnlyList<RegisterBreakpoint> Register { get; private set; } = [];
    public IReadOnlyList<MemoryBreakpoint> Memory { get; private set; } = [];

    public bool AddIfNotExists(Breakpoint breakpoint)
    {
        lock (_lock)
        {
            if (_breakpoints.Contains(breakpoint))
            {
                return false;
            }

            _breakpoints.Add(breakpoint);

            SynchronizeLists();
        }

        return true;
    }

    public bool Remove(Breakpoint breakpoint)
    {
        lock (_lock)
        {
            if (!_breakpoints.Remove(breakpoint))
            {
                return false;
            }

            SynchronizeLists();
        }

        return true;
    }

    public List<Breakpoint> RemoveAll(Predicate<Breakpoint> match)
    {
        lock (_lock)
        {
            var removed = _breakpoints.FindAll(match);

            if (removed.Count > 0)
            {
                _breakpoints.RemoveAll(match);

                SynchronizeLists();
            }

            return removed;
        }
    }

    public bool Replace(Breakpoint original, Breakpoint replacement)
    {
        lock (_lock)
        {
            var existingIndex = _breakpoints.IndexOf(original);

            if (existingIndex < 0)
            {
                return false;
            }

            _breakpoints[existingIndex] = replacement;

            SynchronizeLists();
        }

        return true;
    }

    private void SynchronizeLists()
    {
        Breakpoints = _breakpoints.ToList();
        Register = _breakpoints.OfType<RegisterBreakpoint>().ToList();
        Memory = _breakpoints.OfType<MemoryBreakpoint>().ToList();
    }
}

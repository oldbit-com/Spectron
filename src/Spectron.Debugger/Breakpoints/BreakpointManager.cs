using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Breakpoints;

public enum BreakpointChange
{
    Added,
    Removed,
    Updated
}

public sealed class BreakpointsChangedEventArgs(BreakpointChange change, Breakpoint breakpoint, Breakpoint? original = null) : EventArgs
{
    public BreakpointChange Change { get; } = change;

    /// <summary>
    /// Gets the breakpoint that was added or removed, or the replacement for an update.
    /// </summary>
    public Breakpoint Breakpoint { get; } = breakpoint;

    /// <summary>
    /// Gets the breakpoint that was replaced, for an update.
    /// </summary>
    public Breakpoint? Original { get; } = original;
}

public class BreakpointManager
{
    private readonly BreakpointList  _breakpoints = new();
    private readonly Func<Word>[] _registerValueIndex = new Func<Word>[(int)Register.Last];

    private int _lastId;

    /// <summary>
    /// Gets the user breakpoints, without the single-use breakpoints that the debugger adds when stepping.
    /// </summary>
    public IReadOnlyList<Breakpoint> Breakpoints =>
        _breakpoints.Breakpoints.Where(breakpoint => breakpoint is not RegisterBreakpoint { IsSingleUse: true }).ToList();

    /// <summary>
    /// Raised when a user breakpoint is added, removed or updated, from the thread that made the change.
    /// Not raised for single-use breakpoints.
    /// </summary>
    public event EventHandler<BreakpointsChangedEventArgs>? Changed;

    public BreakpointManager(Z80 cpu) => CreateRegistersIndex(cpu);

    public void UpdateBreakpoint(Breakpoint original, Breakpoint updated)
    {
        updated.Id = original.Id;

        if (_breakpoints.Replace(original, updated))
        {
            Changed?.Invoke(this, new BreakpointsChangedEventArgs(BreakpointChange.Updated, updated, original));
        }
    }

    public void Update(Z80 cpu) => CreateRegistersIndex(cpu);

    public void AddBreakpoint(Breakpoint breakpoint)
    {
        if (breakpoint.Id == 0)
        {
            breakpoint.Id = Interlocked.Increment(ref _lastId);
        }

        if (_breakpoints.AddIfNotExists(breakpoint) && !IsSingleUse(breakpoint))
        {
            Changed?.Invoke(this, new BreakpointsChangedEventArgs(BreakpointChange.Added, breakpoint));
        }
    }

    public void AddSingleUseBreakpoint(Word address) =>
        AddBreakpoint(new RegisterBreakpoint(Register.PC, address) { IsSingleUse = true });

    /// <summary>
    /// Removes single-use breakpoints that were not hit, e.g. when stepping over a call that never returned.
    /// </summary>
    public void RemoveSingleUseBreakpoints() => _breakpoints.RemoveAll(IsSingleUse);

    public void RemoveBreakpoint(Breakpoint breakpoint)
    {
        if (_breakpoints.Remove(breakpoint) && !IsSingleUse(breakpoint))
        {
            Changed?.Invoke(this, new BreakpointsChangedEventArgs(BreakpointChange.Removed, breakpoint));
        }
    }

    public bool ContainsBreakpoint(Register register, Word address) => _breakpoints.Register
        .Any(breakpoint => breakpoint.Register == register && breakpoint.Value == address && !breakpoint.IsSingleUse);

    /// <summary>
    /// Checks the register breakpoints. A breakpoint is hit when the register changes to the breakpoint value.
    /// </summary>
    /// <returns>The breakpoint that was hit, or null.</returns>
    public RegisterBreakpoint? FindRegisterBreakpointHit()
    {
        var breakpoints = _breakpoints.Register;
        RegisterBreakpoint? hit = null;

        // ReSharper disable once ForCanBeConvertedToForeach - foreach in this case is not memory efficient
        for (var index = 0; index < breakpoints.Count; index++)
        {
            var breakpoint = breakpoints[index];

            if (!breakpoint.IsEnabled)
            {
                continue;
            }

            var value = _registerValueIndex[(int)breakpoint.Register]();

            // Every breakpoint tracks its last value, so keep checking after a hit
            if (hit == null && breakpoint.Value == value && breakpoint.LastValue != value)
            {
                hit = breakpoint;
            }

            breakpoint.LastValue = value;
        }

        if (hit is { IsSingleUse: true })
        {
            RemoveBreakpoint(hit);
        }

        return hit;
    }

    public bool IsRegisterBreakpointHit() => FindRegisterBreakpointHit() != null;

    /// <summary>
    /// Checks the memory breakpoints after a write to the given address.
    /// </summary>
    /// <returns>The breakpoint that was hit, or null.</returns>
    public MemoryBreakpoint? FindMemoryBreakpointHit(Word address, IMemory memory)
    {
        var breakpoints = _breakpoints.Memory;

        // ReSharper disable once ForCanBeConvertedToForeach - foreach in this case is not memory efficient
        for (var index = 0; index < breakpoints.Count; index++)
        {
            var breakpoint = breakpoints[index];

            if (!breakpoint.IsEnabled || breakpoint.Address != address)
            {
                continue;
            }

            if (breakpoint.Value == null || breakpoint.Value == memory.Read(address))
            {
                return breakpoint;
            }
        }

        return null;
    }

    private static bool IsSingleUse(Breakpoint breakpoint) => breakpoint is RegisterBreakpoint { IsSingleUse: true };

    private void CreateRegistersIndex(Z80 cpu)
    {
        _registerValueIndex[(int)Register.A] = () => cpu.Registers.A;
        _registerValueIndex[(int)Register.B] = () => cpu.Registers.B;
        _registerValueIndex[(int)Register.C] = () => cpu.Registers.C;
        _registerValueIndex[(int)Register.D] = () => cpu.Registers.D;
        _registerValueIndex[(int)Register.E] = () => cpu.Registers.E;
        _registerValueIndex[(int)Register.H] = () => cpu.Registers.H;
        _registerValueIndex[(int)Register.L] = () => cpu.Registers.L;
        _registerValueIndex[(int)Register.IXH] = () => cpu.Registers.IXH;
        _registerValueIndex[(int)Register.IXL] = () => cpu.Registers.IXL;
        _registerValueIndex[(int)Register.IYH] = () => cpu.Registers.IYH;
        _registerValueIndex[(int)Register.IYL] = () => cpu.Registers.IYL;
        _registerValueIndex[(int)Register.AF] = () => cpu.Registers.AF;
        _registerValueIndex[(int)Register.AFPrime] = () => cpu.Registers.Prime.AF;
        _registerValueIndex[(int)Register.BC] = () => cpu.Registers.BC;
        _registerValueIndex[(int)Register.BCPrime] = () => cpu.Registers.Prime.BC;
        _registerValueIndex[(int)Register.DE] = () => cpu.Registers.DE;
        _registerValueIndex[(int)Register.DEPrime] = () => cpu.Registers.Prime.DE;
        _registerValueIndex[(int)Register.HL] = () => cpu.Registers.HL;
        _registerValueIndex[(int)Register.HLPrime] = () => cpu.Registers.Prime.HL;
        _registerValueIndex[(int)Register.SP] = () => cpu.Registers.SP;
        _registerValueIndex[(int)Register.PC] = () => cpu.Registers.PC;
        _registerValueIndex[(int)Register.IX] = () => cpu.Registers.IX;
        _registerValueIndex[(int)Register.IY] = () => cpu.Registers.IY;
    }
}
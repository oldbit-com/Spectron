using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Debugger.Breakpoints;

public class BreakpointHitEventArgs(Word previousAddress, Breakpoint? breakpoint = null) : EventArgs
{
    /// <summary>
    /// Gets the address of the previous instruction for a register breakpoint,
    /// or of the instruction that wrote to memory for a memory breakpoint.
    /// </summary>
    public Word PreviousAddress { get; } = previousAddress;

    /// <summary>
    /// Gets the breakpoint that was hit.
    /// </summary>
    public Breakpoint? Breakpoint { get; } = breakpoint;
}

public class BreakpointHandler : IDisposable
{
    private Emulator _emulator;
    private Word _previousAddress;
    private Word _currentAddress;

    public BreakpointManager BreakpointManager { get; }

    public Word PreviousAddress { get; private set; }

    public bool IsEnabled
    {
        get;
        set
        {
            field = value;
            HandleIsEnabled();
        }
    }

    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    /// <summary>
    /// Creates a breakpoint handler. Breakpoints are checked only after <see cref="IsEnabled"/> is set to true.
    /// </summary>
    public BreakpointHandler(Emulator emulator)
    {
        _emulator = emulator;

        BreakpointManager = new BreakpointManager(emulator.Cpu);
    }

    public void Update(Emulator emulator)
    {
        UnsubscribeFromEvents(_emulator);

        _emulator = emulator;

        BreakpointManager.Update(emulator.Cpu);

        if (IsEnabled)
        {
            SubscribeToEvents(emulator);
        }
    }

    private void BeforeInstruction(Word pc)
    {
        _previousAddress = _currentAddress;
        _currentAddress = pc;

        var breakpoint = BreakpointManager.FindRegisterBreakpointHit();

        if (breakpoint == null)
        {
            PreviousAddress = pc;
            return;
        }

        _emulator.Break();

        BreakpointHit?.Invoke(this, new BreakpointHitEventArgs(_previousAddress, breakpoint));
    }

    private void MemoryOnMemoryUpdated(Word address, byte value)
    {
        var breakpoint = BreakpointManager.FindMemoryBreakpointHit(address, _emulator.Memory);

        if (breakpoint == null)
        {
            return;
        }

        _emulator.Break();

        BreakpointHit?.Invoke(this, new BreakpointHitEventArgs(_currentAddress, breakpoint));
    }

    private void HandleIsEnabled()
    {
        UnsubscribeFromEvents(_emulator);

        if (IsEnabled)
        {
            SubscribeToEvents(_emulator);
        }
    }

    private void SubscribeToEvents(Emulator emulator)
    {
        emulator.Cpu.BeforeInstruction += BeforeInstruction;
        emulator.Memory.MemoryUpdated += MemoryOnMemoryUpdated;
    }

    private void UnsubscribeFromEvents(Emulator emulator)
    {
        emulator.Cpu.BeforeInstruction -= BeforeInstruction;
        emulator.Memory.MemoryUpdated -= MemoryOnMemoryUpdated;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        UnsubscribeFromEvents(_emulator);
    }
}
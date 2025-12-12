using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Debugger.Breakpoints;

public class BreakpointHitEventArgs(Word previousAddress) : EventArgs
{
    public Word PreviousAddress { get; } = previousAddress;
}

public class BreakpointHandler : IDisposable
{
    private Emulator _emulator;
    private bool _isEnabled;
    private Word _previousAddress;
    private Word _currentAddress;

    public BreakpointManager BreakpointManager { get; }

    public Word PreviousAddress { get; private set; }

    public bool IsBreakpointHit { get; private set; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            HandleIsEnabled();
        }
    }

    public event EventHandler<BreakpointHitEventArgs>? BreakpointHit;

    public BreakpointHandler(Emulator emulator)
    {
        _emulator = emulator;

        BreakpointManager = new BreakpointManager(emulator.Cpu);

        SubscribeToEvents();
    }

    public void Update(Emulator emulator)
    {
        UnsubscribeFromEvents();
        SubscribeToEvents();

        BreakpointManager.Update(emulator.Cpu);

        _emulator = emulator;
    }

    private void BeforeInstruction(Word pc)
    {
        _previousAddress = _currentAddress;
        _currentAddress = pc;

        IsBreakpointHit = BreakpointManager.IsRegisterBreakpointHit();

        if (!IsBreakpointHit)
        {
            PreviousAddress = pc;
            return;
        }

        _emulator.Break();

        BreakpointHit?.Invoke(this, new BreakpointHitEventArgs(_previousAddress));
    }

    private void MemoryOnMemoryUpdated(Word address, byte value)
    {
        IsBreakpointHit = BreakpointManager.IsMemoryBreakpointHit(address, _emulator.Memory);

        if (!IsBreakpointHit)
        {
            return;
        }

        _emulator.Break();

        BreakpointHit?.Invoke(this, new BreakpointHitEventArgs(_currentAddress));
    }

    private void HandleIsEnabled()
    {
        UnsubscribeFromEvents();

        if (IsEnabled)
        {
            SubscribeToEvents();
        }
    }

    private void SubscribeToEvents()
    {
        _emulator.Cpu.BeforeInstruction += BeforeInstruction;
        _emulator.Memory.MemoryUpdated += MemoryOnMemoryUpdated;
    }

    private void UnsubscribeFromEvents()
    {
        _emulator.Cpu.BeforeInstruction -= BeforeInstruction;
        _emulator.Memory.MemoryUpdated -= MemoryOnMemoryUpdated;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        UnsubscribeFromEvents();
    }
}
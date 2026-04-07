using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Debugger.Breakpoints;

public class BreakpointHitEventArgs(Word previousAddress) : EventArgs
{
    public Word PreviousAddress { get; } = previousAddress;
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

    public BreakpointHandler(Emulator emulator)
    {
        _emulator = emulator;

        BreakpointManager = new BreakpointManager(emulator.Cpu);

        SubscribeToEvents(emulator);
    }

    public void Update(Emulator emulator)
    {
        UnsubscribeFromEvents(_emulator);
        SubscribeToEvents(emulator);

        BreakpointManager.Update(emulator.Cpu);

        _emulator = emulator;
    }

    private void BeforeInstruction(Word pc)
    {
        _previousAddress = _currentAddress;
        _currentAddress = pc;

        var isBreakpointHit = BreakpointManager.IsRegisterBreakpointHit();

        if (!isBreakpointHit)
        {
            PreviousAddress = pc;
            return;
        }

        _emulator.Break();

        BreakpointHit?.Invoke(this, new BreakpointHitEventArgs(_previousAddress));
    }

    private void MemoryOnMemoryUpdated(Word address, byte value)
    {
        var iisBreakpointHit = BreakpointManager.IsMemoryBreakpointHit(address, _emulator.Memory);

        if (!iisBreakpointHit)
        {
            return;
        }

        _emulator.Break();

        BreakpointHit?.Invoke(this, new BreakpointHitEventArgs(_currentAddress));
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
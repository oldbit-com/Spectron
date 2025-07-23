using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Breakpoints;

public class BreakpointHandler : IDisposable
{
    private Z80 _cpu;
    private IEmulatorMemory _memory;
    private bool _isEnabled;

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

    public event EventHandler<EventArgs>? BreakpointHit;

    public BreakpointHandler(Z80 cpu, IEmulatorMemory memory)
    {
        BreakpointManager = new BreakpointManager(cpu);
        _cpu = cpu;
        _memory = memory;

        SubscribeToEvents();
    }

    public void Update(Z80 cpu, IEmulatorMemory memory)
    {
        UnsubscribeFromEvents();
        SubscribeToEvents();

        BreakpointManager.Update(cpu);

        _cpu = cpu;
        _memory = memory;
    }

    private void BeforeInstruction(Word pc)
    {
        IsBreakpointHit = BreakpointManager.IsRegisterBreakpointHit();

        if (!IsBreakpointHit)
        {
            PreviousAddress = pc;
            return;
        }

        _cpu.Break();

        BreakpointHit?.Invoke(this, EventArgs.Empty);
    }

    private void MemoryOnMemoryUpdated(Word address)
    {
        IsBreakpointHit = BreakpointManager.IsMemoryBreakpointHit(address, _memory);

        if (!IsBreakpointHit)
        {
            return;
        }

        _cpu.Break();

        BreakpointHit?.Invoke(this, EventArgs.Empty);
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
        _cpu.BeforeInstruction += BeforeInstruction;
        _memory.MemoryUpdated += MemoryOnMemoryUpdated;
    }

    private void UnsubscribeFromEvents()
    {
        _cpu.BeforeInstruction -= BeforeInstruction;
        _memory.MemoryUpdated -= MemoryOnMemoryUpdated;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        UnsubscribeFromEvents();
    }
}
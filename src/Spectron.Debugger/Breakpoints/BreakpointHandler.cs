using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Events;

namespace OldBit.Spectron.Debugger.Breakpoints;

public class BreakpointHandler : IDisposable
{
    private readonly BreakpointManager _breakpointManager;
    private readonly Z80 _cpu;

    public event EventHandler<EventArgs>? BreakpointHit;

    public BreakpointHandler(BreakpointManager breakpointManager, Z80 cpu)
    {
        _breakpointManager = breakpointManager;
        _cpu = cpu;

        _cpu.BeforeInstruction += BeforeInstruction;
    }

    private void BeforeInstruction(BeforeInstructionEventArgs e)
    {
        if (!_breakpointManager.CheckHit())
        {
            return;
        }

        e.IsBreakpoint = true;

        BreakpointHit?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _cpu.BeforeInstruction -= BeforeInstruction;
    }
}
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;
using OldBit.Z80Cpu.Events;

namespace OldBit.Spectron.Debugger.Breakpoints;

public class BreakpointHandler : IDisposable
{
    private Z80 _cpu;

    public BreakpointManager BreakpointManager { get; }

    public event EventHandler<EventArgs>? BreakpointHit;

    public BreakpointHandler(Z80 cpu, IEmulatorMemory memory)
    {
        BreakpointManager = new BreakpointManager(cpu);
        _cpu = cpu;

        _cpu.BeforeInstruction += BeforeInstruction;
    }

    public void Update(Z80 cpu)
    {
        _cpu.BeforeInstruction -= BeforeInstruction;
        BreakpointManager.Update(cpu);

        _cpu = cpu;
    }

    private void BeforeInstruction(BeforeInstructionEventArgs e)
    {
        if (!BreakpointManager.CheckHit())
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
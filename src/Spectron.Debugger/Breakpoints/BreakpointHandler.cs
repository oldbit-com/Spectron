using OldBit.Spectron.Emulation;
using OldBit.Z80Cpu.Events;

namespace OldBit.Spectron.Debugger.Breakpoints;

public class BreakpointHandler : IDisposable
{
    private readonly DebuggerContext _debuggerContext;
    private readonly Emulator _emulator;
    private Word? _ignoreBreakpointAddress = null;

    private HashSet<Word> _addressBreakpoints = [];

    public event EventHandler<EventArgs>? BreakpointHit;

    public BreakpointHandler(DebuggerContext debuggerContext, Emulator emulator)
    {
        _debuggerContext = debuggerContext;
        _emulator = emulator;

        emulator.Cpu.BeforeInstruction += BeforeInstruction;
    }

    private void BeforeInstruction(BeforeInstructionEventArgs e)
    {
        if (_ignoreBreakpointAddress == e.PC)
        {
            _ignoreBreakpointAddress = null;

            return;
        }

        if (!_debuggerContext.HasBreakpoint(e.PC))
        {
            return;
        }

        e.IsBreakpoint = true;

        BreakpointHit?.Invoke(this, EventArgs.Empty);
    }

    public void Resume() => _ignoreBreakpointAddress = _emulator.Cpu.Registers.PC;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _emulator.Cpu.BeforeInstruction -= BeforeInstruction;
    }
}
using OldBit.Spectron.Debugger.Breakpoints;

namespace OldBit.Spectron.Debugger;

public sealed class DebuggerContext
{
    public List<Breakpoint> Breakpoints { get; } = [];

    public List<string> CommandHistory { get; } = [];
}
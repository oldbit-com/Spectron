using OldBit.Spectron.Debugger.Breakpoints;

namespace OldBit.Spectron.Debugger.Messages;

public record BreakpointRemovedMessage(Register Register, Word Address);
using OldBit.Spectron.Debugger.Breakpoints;

namespace OldBit.Spectron.Debugger.Messages;

public record BreakpointUpdatedMessage(Register Register, Word OldAddress, Word NewAddress);
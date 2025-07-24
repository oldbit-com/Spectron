using OldBit.Spectron.Debugger.Breakpoints;

namespace OldBit.Spectron.Debugger.Messages;

public record BreakpointUpdatedMessage(Breakpoint Original, Breakpoint Updated);
using OldBit.Spectron.Debugger.Breakpoints;

namespace OldBit.Spectron.Debugger.Messages;

public record ToggleBreakpointMessage(Register Register, Word Address, bool IsEnabled);
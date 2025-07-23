namespace OldBit.Spectron.Debugger.Breakpoints;

public class RegisterBreakpoint(Register register, Word value) : Breakpoint
{
    public Register Register { get; } = register;

    public Word Value { get; } = value;

    public int? LastValue { get; set; }

    public bool IsSingleUse { get; init; }
}
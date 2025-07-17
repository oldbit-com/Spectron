namespace OldBit.Spectron.Debugger.Breakpoints;

public class RegisterBreakpoint : Breakpoint
{
    public RegisterBreakpoint(Register register, Word address)
    {
        Register = register;

        Value = address;
        ValueAtLastHit = address;
    }

    public Register Register { get; set; }

    public bool IsSingleUse { get; init; }

    public override string ToString() => $"{Register} == ${Value:X4}";
}
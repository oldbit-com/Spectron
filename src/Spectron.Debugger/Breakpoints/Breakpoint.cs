namespace OldBit.Spectron.Debugger.Breakpoints;

public class Breakpoint(Register register, int value)
{
    public Guid Id { get; } = Guid.NewGuid();

    public bool IsEnabled { get; set; } = true;

    public int Value { get; set; } = value;

    public Register Register { get; set; } = register;

    public override string ToString() => $"{Register} == ${Value:X4}";
}
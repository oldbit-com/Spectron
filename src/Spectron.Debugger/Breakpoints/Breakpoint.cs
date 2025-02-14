namespace OldBit.Spectron.Debugger.Breakpoints;

public class Breakpoint(Register register, int value)
{
    public Breakpoint(string register, int value) : this(Enum.Parse<Register>(register, true), value)
    {
    }

    public Guid Id { get; } = Guid.NewGuid();

    public bool IsEnabled { get; set; } = true;

    public int Value { get; set; } = value;

    public Register Register { get; set; } = register;
}
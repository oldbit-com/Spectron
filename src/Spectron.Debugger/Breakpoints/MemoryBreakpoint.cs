namespace OldBit.Spectron.Debugger.Breakpoints;

public class MemoryBreakpoint(Word address, byte? value = null) : Breakpoint
{
    public Word Address { get; } = address;

    public byte? Value { get; } = value;
}
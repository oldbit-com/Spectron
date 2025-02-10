namespace OldBit.Spectron.Debugger.Parser.Values;

public class Integer(int value) : Value
{
    public int Value { get; } = value;
}
namespace OldBit.Spectron.Debugger.Parser.Values;

public class Integer(int value, Type type) : Value
{
    public Integer(int value) : this(value, typeof(int))
    {
    }

    public int Value { get; } = value;

    public Type Type { get; } = type;

    public override string ToString() => Value.ToString();
}
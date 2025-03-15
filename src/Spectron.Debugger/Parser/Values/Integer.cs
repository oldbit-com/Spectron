namespace OldBit.Spectron.Debugger.Parser.Values;

public record Integer(int Value, Type Type) : Value
{
    public Integer(int value) : this(value, typeof(int))
    {
    }

    public int Value { get; } = Value;

    public Type Type { get; } = Type;

    public override string ToString() => Value.ToString();
}
namespace OldBit.Spectron.Debugger.Parser.Values;

public record Register : Value
{
    public string Name { get; }

    public bool Is8Bit { get; }

    public Register(string name)
    {
        Name = name.ToUpperInvariant();

        Is8Bit = Name switch
        {
            "A" => true,
            "B" => true,
            "C" => true,
            "D" => true,
            "E" => true,
            "H" => true,
            "L" => true,
            "I" => true,
            "R" => true,
            "IXH" => true,
            "IXL" => true,
            "IYH" => true,
            "IYL" => true,
            _ => false
        };
    }
}
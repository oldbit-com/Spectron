using OldBit.Spectron.Debugger.Parser.Values;

namespace OldBit.Spectron.Debugger.Parser;

public static class Validators
{
    public static Word GetValidWordOrThrow(Value? value)
    {
        var intValue = value as Integer;

        return intValue?.Value switch
        {
            null => throw new ArgumentException("Argument is null."),
            < 0 or > 0xFFFF => throw new ArgumentException("Argument out of range. Must be between 0 and 65535"),
            _ => (Word)intValue.Value
        };
    }

    public static byte GetValidByteOrThrow(Value? value)
    {
        var intValue = value as Integer;

        return intValue?.Value switch
        {
            null => throw new ArgumentException("Argument is null"),
            < 0 or > 255 => throw new ArgumentException("Argument out of range. Must be between 0 and 255"),
            _ => (byte)intValue.Value
        };
    }
}
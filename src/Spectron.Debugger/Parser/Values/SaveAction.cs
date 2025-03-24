namespace OldBit.Spectron.Debugger.Parser.Values;

public record SaveAction(string FilePath, Word Address, int? Length) : Value;
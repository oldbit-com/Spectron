namespace OldBit.Spectron.Basic.Tokenizer;

public record Token(TokenType TokenType, object? Value = null)
{
    public static readonly Token EndOfLine = new(TokenType.EndOfLine, (byte)0x0D);
    public static readonly Token Separator = new(TokenType.Separator, (byte)0x3A);
    public static Token String(string s) => new(TokenType.String, s);
    public static Token LineNumber(int lineNumber) => new(TokenType.LineNumber, lineNumber);
    public static Token Keyword(byte code) => new(TokenType.Keyword, code);

}

using Antlr4.Runtime;

namespace OldBit.Spectron.Debugger.Parser;

public record SyntaxError(int Line, int Position, string Message)
{
    public override string ToString() => $"{Line}:{Position} {Message}";
}

public class SyntaxErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
{
    public List<SyntaxError> Errors { get; } = [];

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg,
        RecognitionException e)
    {
        Errors.Add(new SyntaxError(line, charPositionInLine, msg));
    }

    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg,
        RecognitionException e)
    {
        var error = new SyntaxError(line, charPositionInLine, msg);

        throw new SyntaxErrorException(error, e);
    }
}
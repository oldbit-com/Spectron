namespace OldBit.Spectron.Debugger.Parser;

public class SyntaxErrorException(SyntaxError error, Exception innerException) :
    Exception(error.Message, innerException)
{
    public SyntaxError Error { get; } = error;
}
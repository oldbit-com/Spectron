namespace OldBit.Spectron.Debugger.Parser;

public class SyntaxErrorException(SyntaxError error, Exception? innerException = null) :
    Exception(error.Message, innerException)
{
    public SyntaxError Error { get; } = error;
}
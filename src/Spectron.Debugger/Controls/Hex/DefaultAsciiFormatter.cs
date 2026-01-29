namespace OldBit.Spectron.Debugger.Controls.Hex;

public sealed class DefaultAsciiFormatter : IAsciiFormatter
{
    public char Format(byte b) => b is >= 32 and <= 126 ? (char)b : '.';
}
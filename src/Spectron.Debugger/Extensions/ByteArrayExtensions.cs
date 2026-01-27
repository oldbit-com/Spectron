namespace OldBit.Spectron.Debugger.Extensions;

public static class ByteArrayExtensions
{
    public static int IndexOfSequence(this ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> pattern, int startIndex)
    {
        if (startIndex < 0 || startIndex > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        if (pattern.IsEmpty || buffer.Length - startIndex < pattern.Length)
        {
            return -1;
        }

        var search = buffer[startIndex..];

        for (var i = 0; i <= search.Length - pattern.Length; i++)
        {
            if (search.Slice(i, pattern.Length).SequenceEqual(pattern))
            {
                return startIndex + i;
            }
        }

        return -1;
    }
}
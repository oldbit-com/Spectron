namespace OldBit.Spectron.Files.IO;

/// <summary>
/// Represents a reader that helps to read bytes data from a stream.
/// </summary>
internal sealed class ByteStreamReader
{
    private readonly Stream _stream;

    /// <summary>
    /// Create a new instance of the byte reader.
    /// </summary>
    /// <param name="stream">The stream that provides data for the reader.</param>
    public ByteStreamReader(Stream stream) => _stream = stream;

    /// <summary>
    /// Reads a byte from the stream and advances the position within the stream by one byte.
    /// </summary>
    /// <returns>The byte retrieved from the stream.</returns>
    /// <exception cref="EndOfStreamException">Thrown when not enough data is in the stream.</exception>
    public byte ReadByte() => !TryReadByte( out var data) ? throw new EndOfStreamException() : data;

    /// <summary>
    /// Reads a word from the stream and advances the position within the stream by two bytes.
    /// </summary>
    /// <returns>The word retrieved from the stream.</returns>
    /// <exception cref="EndOfStreamException">Thrown when not enough data is in the stream.</exception>
    public Word ReadWord() => !TryReadWord(out var data) ? throw new EndOfStreamException() : data;

    /// <summary>
    /// Reads a dword from the stream and advances the position within the stream by four bytes.
    /// </summary>
    /// <returns>The dword retrieved from the stream.</returns>
    public DWord ReadDWord() => !TryReadDWord(out var data) ? throw new EndOfStreamException() : data;

    /// <summary>
    /// Reads a sequence of bytes from the stream and advances the position within the stream by 'count' bytes.
    /// </summary>
    /// <param name="count">The number of bytes to read from the stream.</param>
    /// <returns>An array of bytes retrieved from the stream.</returns>
    /// <exception cref="EndOfStreamException">Thrown when not enough data is in the stream.</exception>
    public byte[] ReadBytes(int count)
    {
        var buffer = new byte[count];
        _stream.ReadExactly(buffer, 0, count);

        return buffer;
    }

    /// <summary>
    /// Reads a sequence of words from the stream and advances the position within the stream by 2 * 'count' bytes.
    /// </summary>
    /// <param name="count">The number of words to read from the stream.</param>
    /// <returns>An array of words retrieved from the stream.</returns>
    public Word[] ReadWords(int count)
    {
        var buffer = new byte[2 * count];
        _stream.ReadExactly(buffer, 0, 2 * count);

        var words = new Word[count];
        for (var i = 0; i < buffer.Length; i +=2)
        {
            words[i / 2] = (Word)(buffer[i] | buffer[i + 1] << 8);
        }

        return words;
    }

    /// <summary>
    /// Attempts to read a byte from the stream and advances the position within the stream by one byte.
    /// </summary>
    /// <param name="result">When this method returns, contains the byte retrieved from the stream,
    /// if the read operation succeeds, or zero if the read operation fails.</param>
    /// <returns>true if the read operation succeeds; otherwise, false.</returns>
    public bool TryReadByte(out byte result)
    {
        result = 0;
        var data = _stream.ReadByte();

        if (data == -1)
        {
            return false;
        }

        result = (byte)data;
        return true;
    }

    /// <summary>
    /// Reads a sequence of bytes from the stream and advances the position within the stream by 'count' bytes.
    /// </summary>
    /// <param name="buffer">An array of bytes retrieved from the stream.</param>
    /// <param name="count">The number of bytes to read.</param>
    /// <returns>The number of bytes read from the stream.</returns>
    public int ReadAtLeast(byte[] buffer, int count) => _stream.ReadAtLeast(buffer, count, false);

    /// <summary>
    /// Reads all the remaining bytes from the stream and returns them as an enumeration of bytes.
    /// </summary>
    /// <returns>An enumeration of bytes representing the remaining data in the stream.</returns>
    public byte[] ReadToEnd()
    {
        var bytes = new List<byte>();
        while (true)
        {
            var data = _stream.ReadByte();
            if (data == -1)
            {
                break;
            }

            bytes.Add((byte)data);
        }

        return bytes.ToArray();
    }

    /// <summary>
    /// Attempts to read a word from the stream and advances the position within the stream by two bytes.
    /// </summary>
    /// <param name="result">When this method returns, contains the word retrieved from the stream,
    /// if the read operation succeeds, or zero if the read operation fails.</param>
    /// <returns>true if the read operation succeeds; otherwise, false.</returns>
    public bool TryReadWord(out Word result)
    {
        var buffer = new byte[2];
        var count = _stream.ReadAtLeast(buffer, 2, false);

        result = 0;
        if (count != 2)
        {
            return false;
        }

        result = (Word)(buffer[0] | buffer[1] << 8);

        return true;
    }

    /// <summary>
    /// Attempts to read a dword from the stream and advances the position within the stream by two bytes.
    /// </summary>
    /// <param name="result">When this method returns, contains the dword retrieved from the stream,
    /// if the read operation succeeds, or zero if the read operation fails.</param>
    /// <returns>true if the read operation succeeds; otherwise, false.</returns>
    public bool TryReadDWord(out DWord result)
    {
        if (!TryReadWord(out var low) || !TryReadWord(out var high))
        {
            result = 0;
            return false;
        }

        result = (DWord)(low | high << 16);

        return true;
    }
}
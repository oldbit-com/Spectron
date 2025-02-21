namespace OldBit.Spectron.Recorder;

internal sealed class WaveFileWriter : IDisposable
{
    private readonly int _sampleRate;
    private readonly short _channels;
    private readonly FileStream _fileStream;
    private readonly BinaryWriter _writer;

    private int _dataSize;
    private bool _isHeaderWritten;

    private const short Pcm = 1;
    private const short BitDepth = 16;

    internal WaveFileWriter(string filePath, int sampleRate, int channels)
    {
        _sampleRate = sampleRate;
        _channels = (short)channels;

        _fileStream = new FileStream(filePath, FileMode.Create);
        _writer = new BinaryWriter(_fileStream);
    }

    internal void Write(IEnumerable<byte> data)
    {
        if (!_isHeaderWritten)
        {
            WriteHeader();
            _isHeaderWritten = true;
        }

        using var iterator = data.GetEnumerator();

        while (true)
        {
            short value;

            if (iterator.MoveNext())
            {
                value = iterator.Current;

                if (iterator.MoveNext())
                {
                    value |= (short)(iterator.Current << 8);
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }

            _writer.Write(value);

            _dataSize += 2;
        }
    }

    internal void Close() => _writer.Close();

    private void WriteHeader()
    {
        // RIFF
        _writer.Write(['R', 'I', 'F', 'F']);
        _writer.Write(0);
        _writer.Write(['W', 'A', 'V', 'E']);

        var byteRate = _sampleRate * _channels * (BitDepth / 8);
        var blockAlign = (short)(_channels * (BitDepth / 8));

        // Format Chunk
        _writer.Write(['f', 'm', 't', ' ']);
        _writer.Write(16);
        _writer.Write(Pcm);
        _writer.Write(_channels);
        _writer.Write(_sampleRate);
        _writer.Write(byteRate);
        _writer.Write(blockAlign);
        _writer.Write(BitDepth);

        // Data Chunk
        _writer.Write(['d', 'a', 't', 'a']);
        _writer.Write(0);
    }

    internal void UpdateHeader()
    {
        _fileStream.Seek(4, SeekOrigin.Begin);
        _writer.Write(36 + _dataSize);

        _fileStream.Seek(40, SeekOrigin.Begin);
        _writer.Write(_dataSize);
    }

    public void Dispose() => Close();
}
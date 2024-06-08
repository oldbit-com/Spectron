namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class BeeperBuffer(int capacity)
{
    private int _bytesLeft;
    private int _position;
    private readonly byte[] _buffer = new byte[capacity];
    private readonly object _bufferLock = new();

    public void Write(byte[] data)
    {
      //  lock (_bufferLock)
        {
            var start = (_position + _bytesLeft) % _buffer.Length;
            var length = Math.Min(data.Length, _buffer.Length - start - 1);
            Array.Copy(data, 0, _buffer, start, length);
            if (length < data.Length)
            {
                Array.Copy(data, length, _buffer, 0, data.Length - length);
            }

            _bytesLeft += data.Length;
        }
    }

    public Func<byte> DefaultAmplitude { get; init; } = () => 0;

    public IEnumerable<byte> GetBuffer(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
       //     lock (_bufferLock)
            {
                if (_bytesLeft == 0)
                {
                    yield return DefaultAmplitude();
                }
                else
                {
                    yield return _buffer[_position];

                    _bytesLeft -= 1;
                    _position += 1;
                    if (_position == _buffer.Length)
                    {
                        _position = 0;
                    }
                }
            }
        }
    }
}
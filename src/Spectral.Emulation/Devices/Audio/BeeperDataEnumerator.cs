using System.Collections;

namespace OldBit.Spectral.Emulation.Devices.Audio;

/// <summary>
/// Represents an infinite circular enumerator that provides beeper data, so the player is always saturated
/// and playing indefinitely, even when beeper is silent.
/// </summary>
/// <param name="capacity">The capacity of the buffer.</param>
/// <param name="defaultValue">The default value to return when buffer is empty.</param>
internal class BeeperDataEnumerator(int capacity, Func<byte> defaultValue) : IEnumerator<byte>
{
    private readonly byte[] _buffer = new byte[capacity];
    private readonly object _bufferLock = new();
    private int _bytesLeft;
    private int _position;

    public void Write(byte[] data)
    {
        lock (_bufferLock)
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

    public bool MoveNext()
    {
        lock (_bufferLock)
        {
            if (_bytesLeft == 0)
            {
                return true;
            }

            _bytesLeft -= 1;
            _position += 1;

            if (_position == _buffer.Length)
            {
                _position = 0;
            }
        }

        return true;
    }

    public void Reset()
    {
    }

    public byte Current
    {
        get
        {
            lock (_bufferLock)
            {
                return _bytesLeft == 0 ? defaultValue() : _buffer[_position];
            }
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
    }
}
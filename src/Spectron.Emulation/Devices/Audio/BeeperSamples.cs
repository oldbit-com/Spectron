namespace OldBit.Spectron.Emulation.Devices.Audio;

/// <summary>
/// Provides a reusable buffer for beeper samples. Once the buffer is primed, it will start reusing existing records.
/// Beeper output means thousands of samples per second, so we need to be efficient.
/// </summary>
internal sealed class BeeperSamples
{
    private readonly List<byte> _buffer = [];
    private int _count;

    internal IEnumerable<byte> Buffer => _buffer.Take(_count);

    internal void Add(short sample)
    {
        if (_buffer.Count > _count)
        {
            _buffer[_count] = (byte)sample;
            _buffer[_count + 1] = (byte)(sample >> 8);
        }
        else
        {
            _buffer.Add((byte)sample);
            _buffer.Add((byte)(sample >> 8));
        }

        _count += 2;
    }

    internal void Reset() => _count = 0;
}
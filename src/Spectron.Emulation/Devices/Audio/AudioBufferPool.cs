namespace OldBit.Spectron.Emulation.Devices.Audio;

internal sealed class AudioBufferPool
{
    private readonly List<AudioBuffer> _pool = [];
    private int _position;

    internal AudioBufferPool(int capacity)
    {
        for (var i = 0; i < capacity; i++)
        {
            _pool.Add(new AudioBuffer());
        }
    }

    internal AudioBuffer GetBuffer()
    {
        var buffer = _pool[_position];
        buffer.Clear();

        _position = (_position + 1) % _pool.Count;

        return buffer;
    }
}
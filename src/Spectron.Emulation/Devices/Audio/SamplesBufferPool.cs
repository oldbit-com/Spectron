namespace OldBit.Spectron.Emulation.Devices.Audio;

internal sealed class SamplesBufferPool
{
    private readonly List<SamplesBuffer> _pool = [];
    private int _position;

    internal SamplesBufferPool(int capacity)
    {
        for (var i = 0; i < capacity; i++)
        {
            _pool.Add(new SamplesBuffer());
        }
    }

    internal SamplesBuffer GetBuffer()
    {
        var buffer = _pool[_position];
        buffer.Reset();

        _position = (_position + 1) % _pool.Count;

        return buffer;
    }
}
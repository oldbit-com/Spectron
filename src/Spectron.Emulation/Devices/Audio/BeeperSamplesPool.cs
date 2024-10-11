namespace OldBit.Spectron.Emulation.Devices.Audio;

internal sealed class BeeperSamplesPool
{
    private readonly List<BeeperSamples> _pool = [];
    private int _position;

    internal BeeperSamplesPool(int capacity)
    {
        for (var i = 0; i < capacity; i++)
        {
            _pool.Add(new BeeperSamples());
        }
    }

    internal BeeperSamples GetBuffer()
    {
        var buffer = _pool[_position];
        buffer.Reset();

        _position = (_position + 1) % _pool.Count;

        return buffer;
    }
}
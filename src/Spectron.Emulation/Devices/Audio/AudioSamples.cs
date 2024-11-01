namespace OldBit.Spectron.Emulation.Devices.Audio;

internal sealed class AudioSamples
{
    private readonly List<int> _buffer = [];

    internal int Count { get; private set; }

    internal void Add(int sample)
    {
        if (_buffer.Count > Count)
        {
            _buffer[Count] = sample;
        }
        else
        {
            _buffer.Add(sample);
        }

        Count += 1;
    }

    internal int this[int index] => _buffer[index];

    internal void Clear() => Count = 0;
}
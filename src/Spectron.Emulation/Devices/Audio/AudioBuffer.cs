namespace OldBit.Spectron.Emulation.Devices.Audio;

public sealed class AudioBuffer
{
    private byte[] _buffer;

    public static readonly AudioBuffer Empty = new(0);

    public byte[] Buffer => _buffer;

    public int Count { get; private set; }

    internal AudioBuffer(int initialSize) => _buffer = new byte [initialSize];

    internal void Add(short sample)
    {
        if (_buffer.Length <= Count)
        {
            Array.Resize(ref _buffer, _buffer.Length + 2);
        }

        _buffer[Count] = (byte)sample;
        _buffer[Count + 1] = (byte)(sample >> 8);

        Count += 2;
    }

    internal void Clear() => Count = 0;
}
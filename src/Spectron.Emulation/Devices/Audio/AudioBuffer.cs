namespace OldBit.Spectron.Emulation.Devices.Audio;

public sealed class AudioBuffer
{
    private byte[] _buffer = [];

    public static readonly AudioBuffer Empty = new();

    public byte[] Buffer => _buffer;

    public int Count { get; private set; }

    internal void Add(short sample)
    {
        if (_buffer.Length <= Count)
        {
            Array.Resize(ref _buffer, _buffer.Length + 16);
        }

        _buffer[Count] = (byte)sample;
        _buffer[Count + 1] = (byte)(sample >> 8);

        Count += 2;
    }

    internal void Clear() => Count = 0;
}
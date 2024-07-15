namespace OldBit.ZXSpectrum.Emulator.Screen;

public class FrameBuffer
{
    public static int Width => DefaultSizes.BorderLeft + DefaultSizes.ContentWidth + DefaultSizes.BorderRight;

    public static int Height => DefaultSizes.BorderTop + DefaultSizes.ContentHeight + DefaultSizes.BorderBottom;

    private readonly byte[] _data;

    public FrameBuffer(Color color)
    {
        Data = Enumerable.Repeat(color, Width * Height).ToArray();

        _data = new byte[Data.Length * 4];
    }

    public void Fill(int start, int count, Color color)
    {
        Array.Fill(Data, color, start, count);
    }

    public byte[] ToBytes()
    {
        for (var i = 0; i < Data.Length; i++)
        {
            _data[4 * i] = Data[i].Red;
            _data[4 * i + 1] = Data[i].Green;
            _data[4 * i + 2] = Data[i].Blue;
            _data[4 * i + 3] = 0xFF;
        }

        return _data;
    }

    public static int GetLineIndex(int line) =>
        (DefaultSizes.BorderLeft + DefaultSizes.ContentWidth + DefaultSizes.BorderRight) * DefaultSizes.BorderTop +
        DefaultSizes.BorderLeft +
        (DefaultSizes.BorderLeft + DefaultSizes.ContentWidth + DefaultSizes.BorderRight) * line;

    // TODO: Convert to plain array of bytes to avoid copying??
    public Color[] Data { get; }
}
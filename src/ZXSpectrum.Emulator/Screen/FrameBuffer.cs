namespace OldBit.ZXSpectrum.Emulator.Screen;

public class FrameBuffer
{
    private readonly byte[] _dataBytes;

    public FrameBuffer(Color color)
    {
        Data = Enumerable.Repeat(color,
            (Constants.BorderLeft + Constants.ContentWidth + Constants.BorderRight) *
            (Constants.BorderTop + Constants.ContentHeight + Constants.BorderBottom)).ToArray();

        _dataBytes = new byte[Data.Length * 4];
    }

    public void Fill(int start, int count, Color color)
    {
        Array.Fill(Data, color, start, count);
    }

    public byte[] ToBytes()
    {
        for (var i = 0; i < Data.Length; i++)
        {
            _dataBytes[4 * i] = Data[i].Red;
            _dataBytes[4 * i + 1] = Data[i].Green;
            _dataBytes[4 * i + 2] = Data[i].Blue;
            _dataBytes[4 * i + 3] = 0xFF;
        }

        return _dataBytes;
    }

    public static int GetLineIndex(int line) =>
        (Constants.BorderLeft + Constants.ContentWidth + Constants.BorderRight) * Constants.BorderTop +
        Constants.BorderLeft +
        (Constants.BorderLeft + Constants.ContentWidth + Constants.BorderRight) * line;

    // TODO: Convert to plain array of bytes to avoid copying
    public Color[] Data { get; }
}
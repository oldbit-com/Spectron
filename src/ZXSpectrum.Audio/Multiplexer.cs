namespace ZXSpectrum.Audio;

public class Multiplexer
{
    public Multiplexer(int sampleRate, int channelCount, AudioFormat format)
    {
        var stream = new MemoryStream();
    }

    private static int ByteLength(AudioFormat format) => format switch
    {
        AudioFormat.Unsigned8Bit => sizeof(byte),
        AudioFormat.Signed16BitIntegerLittleEndian => sizeof(short),
        AudioFormat.Float32BitLittleEndian => sizeof(float),
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}
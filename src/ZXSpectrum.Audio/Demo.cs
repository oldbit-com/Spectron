namespace ZXSpectrum.Audio;

public class Demo
{
    public static byte[] GenerateSinWave(int sampleRate, int channelCount)
    {
        var frequency = 523.3;
        var size = sizeof(byte) * channelCount;
        var buffer = new byte[44000 * channelCount];
        var length = sampleRate / frequency;
        var count = 0;

        for (var i = 0; i < buffer.Length / 2; i++)
        {
            var value = Math.Sin(2 * Math.PI * count / length) * 0.3 + 128;
            for (var channel = 0; channel < channelCount; channel++)
            {
                buffer[i * size + channel] = (byte)value;
            }

            count++;
        }

        return buffer;
    }
}
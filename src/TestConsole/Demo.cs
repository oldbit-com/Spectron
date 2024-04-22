using ZXSpectrum.Audio;

namespace TestConsole;

public class Demo
{
    // public static byte[] GenerateSinWave(int sampleRate, int channelCount, AudioFormat format)
    // {
    //     var bytesPerSample = BytesPerSample(channelCount, format);
    //     var dataLength = channelCount * bytesPerSample;
    //
    //     // 44100 samples per second * 60 seconds
    //
    //
    //     var frequency = 523.3;
    //     var size = sizeof(byte) * channelCount;
    //     var buffer = new byte[44000 * channelCount];
    //     var length = sampleRate / frequency;
    //     var count = 0;
    //
    //     var gain = 1;
    //
    //     for (var i = 0; i < buffer.Length / 2; i++)
    //     {
    //         var multiple = 2 * Math.PI * frequency / sampleRate;
    //         var sampleValue = gain * Math.Sin(nSample * multiple);
    //
    //
    //         var sampleValue = Math.Sin(2 * Math.PI * count / length) * 0.3 + 128;
    //         for (var channel = 0; channel < channelCount; channel++)
    //         {
    //             buffer[i * size + channel] = (byte)value;
    //         }
    //
    //         count++;
    //     }
    //
    //     return buffer;
    // }

    private static int BytesPerSample(int channelCount, AudioFormat format) =>
        format switch
        {
            AudioFormat.Unsigned8Bit => sizeof(byte) * channelCount,
            AudioFormat.Signed16BitIntegerLittleEndian => sizeof(short) * channelCount,
            AudioFormat.Float32BitLittleEndian => sizeof(float) * channelCount,
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
}
namespace TestConsole;

public class SinWaveGenerator(int sampleRate = 44100, int channelCount = 2)
{
    private int _sampleCount;

    public void Generate(float[] buffer, int count)
    {
        var frequency = 523.3;
        var bufferIndex = 0;

        for (var sampleCount = 0; sampleCount < count / channelCount; sampleCount++)
        {
            var multiplier = 2 * Math.PI * frequency / sampleRate;
            var sampleValue = Math.Sin(_sampleCount * multiplier);

            _sampleCount += 1;

            for (var channel = 0; channel < channelCount; channel++)
            {
                buffer[bufferIndex] = (float)sampleValue;
                bufferIndex += 1;
            }
        }
    }
}
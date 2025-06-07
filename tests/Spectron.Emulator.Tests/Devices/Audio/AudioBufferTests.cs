using OldBit.Spectron.Emulation.Devices.Audio;

namespace OldBit.Spectron.Emulator.Tests.Devices.Audio;

public class AudioBufferTests
{
    [Fact]
    public void Buffer_ShouldResize()
    {
        var audioBuffer = new AudioBuffer(10);

        for (short sample = 0; sample < 20; sample++)
        {
            audioBuffer.Add(sample);
        }

        audioBuffer.Count.ShouldBe(40);
        audioBuffer.Buffer.Length.ShouldBe(40);
    }

    [Fact]
    public void Buffer_ShouldHaveCorrectSamplesCount()
    {
        var audioBuffer = new AudioBuffer(10);

        audioBuffer.Add(123);

        audioBuffer.Count.ShouldBe(2);
        audioBuffer.Buffer.Length.ShouldBe(10);
    }

    [Fact]
    public void Buffer_ShouldClear()
    {
        var audioBuffer = new AudioBuffer(10);

        audioBuffer.Add(123);
        audioBuffer.Clear();

        audioBuffer.Count.ShouldBe(0);
        audioBuffer.Buffer.Length.ShouldBe(10);
    }
}
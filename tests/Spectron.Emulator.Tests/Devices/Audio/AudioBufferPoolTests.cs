using OldBit.Spectron.Emulation.Devices.Audio;

namespace OldBit.Spectron.Emulator.Tests.Devices.Audio;

public class AudioBufferPoolTests
{
    [Fact]
    public void BufferPool_ShouldCreateNewBuffers()
    {
        var pool = new AudioBufferPool(4, 10);

        for (var i = 0; i < 4; i++)
        {
            pool.GetBuffer().Buffer.Length.ShouldBe(10);
            pool.GetBuffer().Count.ShouldBe(0);
        }
    }

    [Fact]
    public void BufferPool_ShouldReturnBuffer()
    {
        var pool = new AudioBufferPool(4, 10);

        var buffer = pool.GetBuffer();
        buffer.Add(1);

        buffer = pool.GetBuffer();
        buffer.Add(2);

        buffer = pool.GetBuffer();
        buffer.Add(3);

        buffer = pool.GetBuffer();
        buffer.Add(4);

        buffer = pool.GetBuffer();
        buffer.Buffer[0].ShouldBe(1);

        buffer = pool.GetBuffer();
        buffer.Buffer[0].ShouldBe(2);

        buffer = pool.GetBuffer();
        buffer.Buffer[0].ShouldBe(3);

        buffer = pool.GetBuffer();
        buffer.Buffer[0].ShouldBe(4);

        buffer = pool.GetBuffer();
        buffer.Buffer[0].ShouldBe(1);
    }
}
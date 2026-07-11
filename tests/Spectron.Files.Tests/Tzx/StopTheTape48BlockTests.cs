using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class StopTheTape48BlockTests
{
    [Fact]
    public void StopTheTape48Block_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([0x00, 0x00, 0x00, 0x00]);
        var reader = new ByteStreamReader(stream);
        var block = new StopTheTape48Block(reader);

        Assert.Throws<EndOfStreamException>(() => reader.ReadByte());
    }

    [Fact]
    public void StopTheTape48Block_ShouldSerializeToBytes()
    {
        var block = new StopTheTape48Block();

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x2A, 0x00, 0x00, 0x00, 0x00 });
    }
}
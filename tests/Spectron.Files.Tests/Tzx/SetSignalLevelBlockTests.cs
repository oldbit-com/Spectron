using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class SetSignalLevelBlockTests
{
    [Fact]
    public void SetSignalLevelBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new SetSignalLevelBlock();

        block.Level.ShouldBe(SignalLevel.Low);
    }

    [Fact]
    public void SetSignalLevelBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([0x01, 0x00, 0x00, 0x00, 0x01]);
        var block = new SetSignalLevelBlock(new ByteStreamReader(stream));

        block.Level.ShouldBe(SignalLevel.High);
    }

    [Fact]
    public void SetSignalLevelBlock_ShouldSerializeToBytes()
    {
        var block = new SetSignalLevelBlock{ Level = SignalLevel.High };

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x2B, 0x01, 0x00, 0x00, 0x00, 0x01 });
    }
}
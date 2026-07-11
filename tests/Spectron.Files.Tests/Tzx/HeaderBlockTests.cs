using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tests.Tzx;

public class HeaderBlockTests
{
    [Fact]
    public void HeaderBlock_NewInstance_ShouldCreateWithDefaultValues()
    {
        var block = new HeaderBlock();

        block.Signature.ShouldBe(HeaderBlock.TzxSignature);
        block.EotMarker.ShouldBe(HeaderBlock.TzxEotMarker);
        block.VerMajor.ShouldBe(HeaderBlock.TzxVerMajor);
        block.VerMinor.ShouldBe(HeaderBlock.TzxVerMinor);
    }

    [Fact]
    public void HeaderBlock_ShouldDeserializeFromStream()
    {
        using var stream = new MemoryStream([
            0x5A, 0x58, 0x54, 0x61, 0x70, 0x65, 0x21, 0x1A, 0x01, 0x0A
        ]);
        var block = new HeaderBlock(new ByteStreamReader(stream));

        block.Signature.ShouldBe(HeaderBlock.TzxSignature);
        block.EotMarker.ShouldBe(HeaderBlock.TzxEotMarker);
        block.VerMajor.ShouldBe(HeaderBlock.TzxVerMajor);
        block.VerMinor.ShouldBe(0x0A);
    }

    [Fact]
    public void HeaderBlock_ShouldSerializeToBytes()
    {
        var block = new HeaderBlock();

        var result = FileDataSerializer.Serialize(block);

        result.ShouldBeEquivalentTo(new byte[] { 0x5A, 0x58, 0x54, 0x61, 0x70, 0x65, 0x21, 0x1A, 0x01, 0x14 });
    }
}
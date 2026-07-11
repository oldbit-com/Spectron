using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class SpecRegsBlockTests
{
    [Fact]
    public void SpecRegs_ShouldConvertToBytes()
    {
        var regs = GetSpecRegsBlock();
        using var writer = new MemoryStream();

        regs.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe(8 + 8);

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe(0x52435053);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe(8);

        // Data
        data[8].ShouldBe(1);
        data[9].ShouldBe(2);
        data[10].ShouldBe(3);
        data[11].ShouldBe(4);
        data[12..16].ToArray().ShouldBeEquivalentTo(new byte[] { 0, 0, 0, 0 });
    }

    [Fact]
    public void SpecRegs_ShouldConvertFromBytes()
    {
        var regsData = GetSpecRegsBlockData();
        using var memoryStream = new MemoryStream(regsData);
        var reader = new ByteStreamReader(memoryStream);

        var regs = SpecRegsBlock.Read(reader, regsData.Length);

        regs.Border.ShouldBe(1);
        regs.Port7FFD.ShouldBe(2);
        regs.Port1FFD.ShouldBe(3);
        regs.PortFE.ShouldBe(4);
        regs.Reserved.ShouldBeEquivalentTo(new byte[] { 0, 0, 0, 0 });
    }

    private static byte[] GetSpecRegsBlockData()
    {
        var registers = GetSpecRegsBlock();
        using var writer = new MemoryStream();

        registers.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private static SpecRegsBlock GetSpecRegsBlock() => new()
    {
        Border = 1,
        Port7FFD = 2,
        Port1FFD = 3,
        PortFE = 4,
    };
}
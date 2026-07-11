using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class Z80RegsBlockTests
{
    [Fact]
    public void Z80Regs_ShouldConvertToBytes()
    {
        var regs = GetZ80RegsBlock();
        using var writer = new MemoryStream();

        regs.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe(8 + 37);

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe(0x5230385A);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe(37);

        // Data
        BitConverter.ToUInt16(data[8..10].ToArray()).ShouldBe(0x1234);
        BitConverter.ToUInt16(data[10..12].ToArray()).ShouldBe(0x5678);
        BitConverter.ToUInt16(data[12..14].ToArray()).ShouldBe(0x9ABC);
        BitConverter.ToUInt16(data[14..16].ToArray()).ShouldBe(0xDEF0);
        BitConverter.ToUInt16(data[16..18].ToArray()).ShouldBe(0x1234);
        BitConverter.ToUInt16(data[18..20].ToArray()).ShouldBe(0x5678);
        BitConverter.ToUInt16(data[20..22].ToArray()).ShouldBe(0x9ABC);
        BitConverter.ToUInt16(data[22..24].ToArray()).ShouldBe(0xDEF0);
        BitConverter.ToUInt16(data[24..26].ToArray()).ShouldBe(0x1234);
        BitConverter.ToUInt16(data[26..28].ToArray()).ShouldBe(0x5678);
        BitConverter.ToUInt16(data[28..30].ToArray()).ShouldBe(0xDEF0);
        BitConverter.ToUInt16(data[30..32].ToArray()).ShouldBe(0x9ABC);
        data[32].ShouldBe(0x12);
        data[33].ShouldBe(0x34);
        data[34].ShouldBe(0x01);
        data[35].ShouldBe(0x01);
        data[36].ShouldBe(0x02);
        BitConverter.ToUInt32(data[37..41].ToArray()).ShouldBe(1985);
        data[41].ShouldBe(0x01);
        data[42].ShouldBe(0x02);
        BitConverter.ToUInt16(data[43..45].ToArray()).ShouldBe(0x1234);
    }

    [Fact]
    public void Z80Regs_ShouldConvertFromBytes()
    {
        var regsData = GetZ80RegsBlockData();
        using var memoryStream = new MemoryStream(regsData);
        var reader = new ByteStreamReader(memoryStream);

        var regs = Z80RegsBlock.Read(reader, regsData.Length);

        regs.AF.ShouldBe(0x1234);
        regs.BC.ShouldBe(0x5678);
        regs.DE.ShouldBe(0x9ABC);
        regs.HL.ShouldBe(0xDEF0);
        regs.AF1.ShouldBe(0x1234);
        regs.BC1.ShouldBe(0x5678);
        regs.DE1.ShouldBe(0x9ABC);
        regs.HL1.ShouldBe(0xDEF0);
        regs.IX.ShouldBe(0x1234);
        regs.IY.ShouldBe(0x5678);
        regs.SP.ShouldBe(0xDEF0);
        regs.PC.ShouldBe(0x9ABC);
        regs.I.ShouldBe(0x12);
        regs.R.ShouldBe(0x34);
        regs.IFF1.ShouldBe(0x01);
        regs.IFF2.ShouldBe(0x01);
        regs.IM.ShouldBe(0x02);
        regs.CyclesStart.ShouldBe(1985);
        regs.HoldIntReqCycles.ShouldBe(0x01);
        regs.Flags.ShouldBe(Z80RegsBlock.FlagsHalted);
        regs.MemPtr.ShouldBe(0x1234);
    }

    private static byte[] GetZ80RegsBlockData()
    {
        var registers = GetZ80RegsBlock();
        using var writer = new MemoryStream();

        registers.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private static Z80RegsBlock GetZ80RegsBlock() => new()
    {
        AF = 0x1234,
        BC = 0x5678,
        DE = 0x9ABC,
        HL = 0xDEF0,
        AF1 = 0x1234,
        BC1 = 0x5678,
        DE1 = 0x9ABC,
        HL1 = 0xDEF0,
        IX = 0x1234,
        IY = 0x5678,
        SP = 0xDEF0,
        PC = 0x9ABC,
        I = 0x12,
        R = 0x34,
        IFF1 = 0x01,
        IFF2 = 0x01,
        IM = 0x02,
        CyclesStart = 1985,
        HoldIntReqCycles = 0x01,
        Flags = Z80RegsBlock.FlagsHalted,
        MemPtr = 0x1234
    };
}
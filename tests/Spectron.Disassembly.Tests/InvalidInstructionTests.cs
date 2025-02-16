namespace OldBit.Spectron.Disassembly.Tests;

public class InvalidInstructionTests
{
    [Theory]
    [InlineData(new byte[] { 0xED, 0x00 })]
    [InlineData(new byte[] { 0xED, 0x4E })]
    public void InvalidExtendedInstruction_ShouldDisassemblyAsUnknown(byte[] data)
    {
        var disassembler = new Disassembler(data, 0, 1);

        var code = disassembler.Disassemble();

        code.Count.ShouldBe(1);
        code[0].ToString().ShouldBe("???");
        code[0].IsInvalid.ShouldBeTrue();
        code[0].ByteCodes.ShouldBe(data);
    }

    [Theory]
    [InlineData(new byte[] { 0xDD, 0xDD, 0xDD, 0xDD, 0x09 })]
    [InlineData(new byte[] { 0xDD, 0xFD, 0xFD, 0xDD, 0x09 })]
    [InlineData(new byte[] { 0xFD, 0xFD, 0xFD, 0xDD, 0x09 })]
    public void MultipleIndexPrefixes_ShouldDisassemblyAsNop(byte[] data)
    {
        var disassembler = new Disassembler(data, 0, 4);

        var code = disassembler.Disassemble();

        code.Count.ShouldBe(4);
        code[0].ToString().ShouldBe("NOP?");
        code[0].IsInvalid.ShouldBeTrue();
        code[0].Address.ShouldBe((ushort)0);
        code[0].ByteCodes.ShouldBe(data[..1]);

        code[1].ToString().ShouldBe("NOP?");
        code[1].IsInvalid.ShouldBeTrue();
        code[1].Address.ShouldBe((ushort)1);
        code[1].ByteCodes.ShouldBe(data[1..2]);

        code[2].ToString().ShouldBe("NOP?");
        code[2].IsInvalid.ShouldBeTrue();
        code[2].Address.ShouldBe((ushort)2);
        code[2].ByteCodes.ShouldBe(data[2..3]);

        code[3].ToString().ShouldBe("ADD IX,BC");
        code[3].IsInvalid.ShouldBeFalse();
        code[3].Address.ShouldBe((ushort)3);
        code[3].ByteCodes.ShouldBe(data[3..]);
    }

    [Theory]
    [InlineData(new byte[] { 0xDD, 0xED, 0x46 })]
    [InlineData(new byte[] { 0xFD, 0xED, 0x46  })]
    public void IndexExtendedInstruction_ShouldDisassemblyAsUnknown(byte[] data)
    {
        var disassembler = new Disassembler(data, 0, 2);

        var code = disassembler.Disassemble();

        code.Count.ShouldBe(2);
        code[0].ToString().ShouldBe("NOP?");
        code[0].IsInvalid.ShouldBeTrue();
        code[0].ByteCodes.ShouldBe([data[0]]);

        code[1].ToString().ShouldBe("IM 0");
        code[1].IsInvalid.ShouldBeFalse();
        code[1].ByteCodes.ShouldBe(data[1..]);
    }
}
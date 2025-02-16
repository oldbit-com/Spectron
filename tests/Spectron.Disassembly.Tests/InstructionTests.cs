using OldBit.Spectron.Disassembly.Tests.Support;

namespace OldBit.Spectron.Disassembly.Tests;

public class InstructionTests
{
    [Theory]
    [MemberData(nameof(BitInstructionsTestData))]
    public void BitInstructions_ShouldDisassembly(InstructionTestCase test)
    {
        RunTestCase(test);
    }

    [Theory]
    [MemberData(nameof(MainInstructionsTestData))]
    public void MainInstructions_ShouldDisassembly(InstructionTestCase test)
    {
        RunTestCase(test);
    }

    [Theory]
    [MemberData(nameof(ExtendedInstructionsTestData))]
    public void ExtendedInstructions_ShouldDisassembly(InstructionTestCase test)
    {
        RunTestCase(test);
    }

    [Theory]
    [MemberData(nameof(IndexInstructionsTestData))]
    public void IndexInstructions_ShouldDisassembly(InstructionTestCase test)
    {
        RunTestCase(test);
    }

    [Fact]
    public void AllMainInstructions_ShouldDisassembly()
    {
        for (var byteCode = 0; byteCode < 256; byteCode++)
        {
            if (byteCode is 0xCB or 0xED or 0xDD or 0xFD)
            {
                continue;
            }

            var disassembler = new Disassembler([(byte)byteCode, 0, 0], 0, 1);
            var code = disassembler.Disassemble();

            code.Count.ShouldBe(1);
        }
    }

    private static void RunTestCase(InstructionTestCase test)
    {
        var disassembler = new Disassembler(test.Bytes, test.StartAddress, 1);

        var code = disassembler.Disassemble();

        code.Count.ShouldBe(1);
        code[0].ToString().ShouldBe(test.Instruction);
        code[0].IsUndocumented.ShouldBe(test.IsUndocumented);
        code[0].ByteCodes.ShouldBe(test.Bytes.Skip(test.StartAddress).ToArray());
    }

    public static IEnumerable<object[]> BitInstructionsTestData => TestCaseLoader.GetTestData("test_bit_instructions.json");

    public static IEnumerable<object[]> MainInstructionsTestData => TestCaseLoader.GetTestData("test_main_instructions.json");

    public static IEnumerable<object[]> ExtendedInstructionsTestData => TestCaseLoader.GetTestData("test_ed_instructions.json");

    public static IEnumerable<object[]> IndexInstructionsTestData => TestCaseLoader.GetTestData("test_xy_instructions.json");
}
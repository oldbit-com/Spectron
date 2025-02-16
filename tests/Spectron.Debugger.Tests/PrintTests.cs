using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;
using Shouldly;

namespace OldBit.Spectron.Debugger.Tests;

public class PrintTests
{
    [Fact]
    public void Print_ShouldOutputRegisterValue()
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory)
        {
            Registers =
            {
                AF = 0x0102,
                BC = 0x0304,
                DE = 0x0506,
                HL = 0x0708,
                IX = 0x090A,
                IY = 0x0B0C,
                SP = 0x0D0E,
                PC = 0x0F10,
            }
        };
        var output = new TestPrintOutput();

        var interpreter = new Interpreter(cpu, memory, output);
        interpreter.Execute("PRINT AF,BC,DE,HL,SP,PC,IX,IY,A,B,C,D,E,H,L,IXH,IXL,IYH,IYL");

        output.Lines.Count.ShouldBe(19);
        output.Lines.ShouldBeEquivalentTo(new List<string>
        {
            "AF=$0102  (258)",
            "BC=$0304  (772)",
            "DE=$0506  (1286)",
            "HL=$0708  (1800)",
            "SP=$0D0E  (3342)",
            "PC=$0F10  (3856)",
            "IX=$090A  (2314)",
            "IY=$0B0C  (2828)",
            "A=$01  (1)",
            "B=$03  (3)",
            "C=$04  (4)",
            "D=$05  (5)",
            "E=$06  (6)",
            "H=$07  (7)",
            "L=$08  (8)",
            "IXH=$09  (9)",
            "IXL=$0A  (10)",
            "IYH=$0B  (11)",
            "IYL=$0C  (12)"
        });
    }
}
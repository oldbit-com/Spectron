using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

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

        var interpreter = new Interpreter(cpu, memory, Substitute.For<IBus>(), output);
        var result = interpreter.Execute("PRINT AF,BC,DE,HL,SP,PC,IX,IY,A,B,C,D,E,H,L,IXH,IXL,IYH,IYL");

        result.ShouldBeOfType<Print>();
        var printResult = (Print)result;

        printResult.Values.Count.ShouldBe(19);
        printResult.Values.ShouldAllBe(x => x is Register);
    }
}
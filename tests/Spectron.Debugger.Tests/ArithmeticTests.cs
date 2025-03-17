using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Tests;

public class ArithmeticTests
{
    [Theory]
    [InlineData("? 1+2", 3)]
    [InlineData("? 1-2", -1)]
    [InlineData("? 12-(10+2)", 0)]
    [InlineData("? 12-10+2", 4)]
    [InlineData("? 3*2", 6)]
    [InlineData("? 1+3*2", 7)]
    [InlineData("? 3*2+1", 7)]
    [InlineData("? (1+3)*2", 8)]
    [InlineData("? 3*(2+1)", 9)]
    [InlineData("? 4*5/2", 10)]
    [InlineData("? 4*(6/2)", 12)]
    [InlineData("? 4*6/2", 12)]
    [InlineData("? 3+5*2-8/4", 11)]
    public void ArithmeticOperation_ShouldCalculateCorrectResult(string expression, int expectedResult)
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory);

        var interpreter = new Interpreter(cpu, memory, Substitute.For<IBus>(), new TestPrintOutput());
        var result = interpreter.Execute(expression);

        result.ShouldNotBeNull();
        result.ShouldBeOfType<Print>();

        var values = ((Print)result).Values;
        values.Count.ShouldBe(1);

        ((Integer)values[0]!).Value.ShouldBe(expectedResult);
    }
}
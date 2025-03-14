using NSubstitute;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Tests;

public class AssignTests
{
    private readonly Z80 _cpu;
    private readonly IMemory _memory;

    public AssignTests()
    {
        var rom = new byte[16384];
        _memory = new Memory48K(rom);
        _cpu = new Z80(_memory);
    }

    [Theory]
    [InlineData("BC = 16384")]
    [InlineData("BC=0x4000")]
    [InlineData("BC = 4000H")]
    [InlineData("BC  =$4000")]
    [InlineData("BC=  0b0100000000000000")]
    public void Assign_ShouldSetBCRegisterValue(string statement)
    {
        var interpreter = new Interpreter(_cpu, _memory, Substitute.For<IBus>(), new TestPrintOutput());
        var assignResult = interpreter.Execute(statement);

        assignResult.ShouldBeOfType<Success>();

        _cpu.Registers.BC.ShouldBe((Word)16384);
    }

    [Theory]
    [InlineData("A=1", "A", 1)]
    [InlineData("B=2", "B", 2)]
    [InlineData("C=3", "C", 3)]
    [InlineData("D=4", "D", 4)]
    [InlineData("E=5", "E", 5)]
    [InlineData("H=6", "H", 6)]
    [InlineData("L=7", "L", 7)]
    [InlineData("I=8", "I", 8)]
    [InlineData("R=9", "R", 9)]
    [InlineData("IXH=10", "IXH", 10)]
    [InlineData("IXL=11", "IXL", 11)]
    [InlineData("IYH=12", "IYH", 12)]
    [InlineData("IYL=13", "IYL", 13)]
    [InlineData("AF=14", "AF", 14)]
    [InlineData("AF'=15", "AF'", 15)]
    [InlineData("BC=16", "BC", 16)]
    [InlineData("BC'=17", "BC'", 17)]
    [InlineData("DE=18", "DE", 18)]
    [InlineData("DE'=19", "DE'", 19)]
    [InlineData("HL=20", "HL", 20)]
    [InlineData("HL'=21", "HL'", 21)]
    [InlineData("IX=22", "IX", 22)]
    [InlineData("IY=23", "IY", 23)]
    [InlineData("PC=24", "PC", 24)]
    [InlineData("SP=25", "SP", 25)]
    public void Assign_ShouldSetRegisterValue(string statement, string register, int value)
    {
        var interpreter = new Interpreter(_cpu, _memory, Substitute.For<IBus>(), new TestPrintOutput());
        var assignResult = interpreter.Execute(statement);

        assignResult.ShouldBeOfType<Success>();

        _cpu.GetRegisterValue(register).ShouldBe(value);
    }
}
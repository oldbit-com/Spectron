using NSubstitute;
using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Tests;

public class InTests
{
    [Theory]
    [InlineData("IN 16384", 0xA6)]
    [InlineData("IN HL", 0x6A)]
    public void In_ShouldReturnBusValue(string statement, byte expected)
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory);
        var bus = Substitute.For<IBus>();
        cpu.AddBus(bus);

        bus.Read(16384).Returns((byte)0xA6);
        bus.Read(16385).Returns((byte)0x6A);
        cpu.Registers.HL = 16385;

        var interpreter = new Interpreter(cpu, memory, bus, new TestPrintOutput());
        var inResult = interpreter.Execute(statement);

        inResult.ShouldBeOfType<Integer>();
        var value = (Integer)inResult;

        value.Value.ShouldBe(expected);
    }
}
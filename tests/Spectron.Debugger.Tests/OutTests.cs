using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Tests;

public class OutTests
{
    [Theory]
    [InlineData("OUT 254,10")]
    [InlineData("OUT 0xFE,0Ah")]
    [InlineData("OUT FEH,$0A")]
    [InlineData("OUT $FE,0X0A")]
    [InlineData("OUT 0b11111110,1010b")]
    public void Out_ShouldUpdateBus(string statement)
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory);
        var bus = Substitute.For<IBus>();
        cpu.AddBus(bus);

        var interpreter = new Interpreter(cpu, memory, bus, new TestPrintOutput());
        var outResult = interpreter.Execute(statement);

        outResult.ShouldBeOfType<Success>();

        bus.Received().Write(254, 10);
    }
}
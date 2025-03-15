using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Tests;

public class PeekTests
{
    [Theory]
    [InlineData("PEEK 16384", 0xA6)]
    [InlineData("PEEK HL", 0x6A)]
    public void Peek_ShouldReturnMemoryValue(string statement, byte expected)
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory);

        memory.Write(16384, 0xA6);
        memory.Write(16385, 0x6A);
        cpu.Registers.HL = 16385;

        var interpreter = new Interpreter(cpu, memory, Substitute.For<IBus>(), new TestPrintOutput());
        var peekResult = interpreter.Execute(statement);

        peekResult.ShouldBeOfType<Integer>();
        var value = (Integer)peekResult;

        value.Value.ShouldBe(expected);
    }
}
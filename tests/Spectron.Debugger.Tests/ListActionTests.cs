using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Tests;

public class ListActionTests
{
    [Fact]
    public void List_ShouldReturnProgramCounter()
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory)
        {
            Registers =
            {
                PC = 0xF000
            }
        };

        var interpreter = new Interpreter(cpu, memory, Substitute.For<IBus>(), new TestPrintOutput());
        var listResult = interpreter.Execute("LIST");

        listResult.ShouldBeOfType<ListAction>();
        ((ListAction)listResult).Address.ShouldBe(cpu.Registers.PC);
    }

    [Fact]
    public void List_WithAddress_ShouldReturnProgramCounter()
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory);

        var interpreter = new Interpreter(cpu, memory, Substitute.For<IBus>(), new TestPrintOutput());
        var listResult = interpreter.Execute("LIST 0xF000");

        listResult.ShouldBeOfType<ListAction>();
        ((ListAction)listResult).Address.ShouldBe((Word)0xF000);
    }
}
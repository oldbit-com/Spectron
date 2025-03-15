using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Tests;

public class GotoActionTests
{
    [Fact]
    public void GoTo_ShouldUpdateProgramCounter()
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory);

        var interpreter = new Interpreter(cpu, memory, Substitute.For<IBus>(), new TestPrintOutput());
        var gotoResult = interpreter.Execute("GOTO 32768");

        gotoResult.ShouldBeOfType<GotoAction>();
        ((GotoAction)gotoResult).Address.ShouldBe((Word)32768);
        cpu.Registers.PC.ShouldBe((Word)32768);
    }
}
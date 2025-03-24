using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Parser.Values;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Debugger.Tests;

public class SaveActionTest
{
    [Theory]
    [InlineData("SAVE \"dump.bin\"", 0, null)]
    [InlineData("SAVE 'dump.bin'", 0, null)]
    [InlineData("SAVE \"dump.bin\" 16384", 16384, null)]
    [InlineData("SAVE \"dump.bin\" 16384,256", 16384, 256)]
    public void Save_ShouldReturnSaveAction(string statement, Word expectedAddress, int? expectedLength)
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory);

        var interpreter = new Interpreter(cpu, memory, Substitute.For<IBus>(), new TestPrintOutput());
        var saveResult = interpreter.Execute(statement);

        saveResult.ShouldBeOfType<SaveAction>();
        ((SaveAction)saveResult).FilePath.ShouldBe("dump.bin");
        ((SaveAction)saveResult).Address.ShouldBe(expectedAddress);
        ((SaveAction)saveResult).Length.ShouldBe(expectedLength);
    }
}
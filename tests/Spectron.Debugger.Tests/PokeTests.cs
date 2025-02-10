using OldBit.Spectron.Debugger.Parser;
using OldBit.Spectron.Debugger.Tests.Fixtures;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Z80Cpu;
using Shouldly;

namespace OldBit.Spectron.Debugger.Tests;

public class PokeTests
{
    [Theory]
    [InlineData("POKE 16384,255")]
    [InlineData("POKE 0x4000,FFh")]
    [InlineData("POKE 4000H,$FF")]
    [InlineData("POKE $4000,0XFF")]
    [InlineData("POKE 0b0100000000000000,11111111b")]
    public void Poke_ShouldUpdateMemory(string statement)
    {
        var rom = new byte[16384];
        var memory = new Memory48K(rom);
        var cpu = new Z80(memory);

        var interpreter = new Interpreter(cpu, memory, new TestPrintOutput());
        interpreter.Execute(statement);

        var result = memory.Read(16384);
        result.ShouldBe((byte)0xFF);
    }
}
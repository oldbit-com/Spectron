using OldBit.Spectron.Emulation.Devices.Storage.SD;
using Shouldly;

namespace OldBit.Spectron.Emulator.Tests.Devices.Storage.SD;

public class CommandTests
{
    [Fact]
    public void IncompleteCommand_ShouldBeCreated()
    {
        var result = Command.TryCreateCommand(0x40, out var command);

        result.ShouldBeTrue();
        command.ShouldNotBeNull();
        command.IsReady.ShouldBeFalse();
    }

    [Fact]
    public void CompleteCommand_ShouldBeCreated()
    {
        Command.TryCreateCommand(0x40, out var command);

        command.ShouldNotBeNull();

        command.ProcessNextByte(0x00);
        command.ProcessNextByte(0x00);
        command.ProcessNextByte(0x00);
        command.ProcessNextByte(0x00);
        command.ProcessNextByte(0x95);
        command.IsReady.ShouldBeTrue();
    }
}
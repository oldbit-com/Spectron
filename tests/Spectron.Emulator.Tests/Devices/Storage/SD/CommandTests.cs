using OldBit.Spectron.Emulation.Devices.DivMmc.SD;

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

        command.NextByte(0x00);
        command.NextByte(0x00);
        command.NextByte(0x00);
        command.NextByte(0x00);
        command.NextByte(0x95);
        command.IsReady.ShouldBeTrue();
    }
}
using OldBit.Spectron.Emulation.Devices.Beta128.Controller.Commands;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128.Controller.Commands;

public class CommandTests
{
    [Theory]
    [InlineData(0x00)]
    [InlineData(0x0F)]
    public void RestoreCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type1);
        command.IsRestore.ShouldBe(true);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0x10)]
    [InlineData(0x1F)]
    public void SeekCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type1);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(true);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0x20)]
    [InlineData(0x3F)]
    public void StepCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type1);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(true);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0x40)]
    [InlineData(0x5F)]
    public void StepInCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type1);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(true);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0x60)]
    [InlineData(0x7F)]
    public void StepOutCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type1);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(true);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0x80)]
    [InlineData(0x9F)]
    public void RadSectorCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type2);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(true);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0xA0)]
    [InlineData(0xBF)]
    public void WriteSectorCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type2);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(true);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0xC0)]
    [InlineData(0xC4)]
    public void ReadAddressCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type3);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(true);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0xE0)]
    [InlineData(0xE4)]
    public void ReadTrackCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type3);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(true);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0xF0)]
    [InlineData(0xF4)]
    public void WriteTrackCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type3);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(true);
        command.IsForceInterrupt.ShouldBe(false);
    }

    [Theory]
    [InlineData(0xD0)]
    [InlineData(0xDF)]
    public void ForceInterruptCommand_ShouldMatch(byte value)
    {
        var command = new Command(value);

        command.Type.ShouldBe(CommandType.Type4);
        command.IsRestore.ShouldBe(false);
        command.IsSeek.ShouldBe(false);
        command.IsStep.ShouldBe(false);
        command.IsStepIn.ShouldBe(false);
        command.IsStepOut.ShouldBe(false);
        command.IsReadSector.ShouldBe(false);
        command.IsWriteSector.ShouldBe(false);
        command.IsReadAddress.ShouldBe(false);
        command.IsReadTrack.ShouldBe(false);
        command.IsWriteTrack.ShouldBe(false);
        command.IsForceInterrupt.ShouldBe(true);
    }
}
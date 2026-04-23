using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Integration.Tests.Fixtures;

namespace OldBit.Spectron.Integration.Tests;

[Trait("Category", "Integration")]
[MeasureTime]
public class Halt2IntTests : IntegrationTestBase
{
    [Fact]
    public void RunHalt2Int_48K()
    {
        var emulator = Loader.EnterLoadCommand(ComputerType.Spectrum48K);

        emulator.IsFloatingBusEnabled = true;
        emulator.TapeManager.TapeLoadSpeed = TapeSpeed.Instant;

        var testFile = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/halt2int.tap");
        emulator.TapeManager.InsertTape(testFile);

        emulator.RunFrames(180);

        SaveOutput(emulator.ScreenBuffer.FrameBuffer, "Halt2Int48k.png");

        var hash = emulator.ScreenBuffer.FrameBuffer.CalculateHash();
        hash.ShouldBe("EE2B26C1140905104B02B97281ADAF1EAE7FBC028126584111A710D81BD0A30C");
    }

    [Fact]
    public void RunHalt2Int_128K()
    {
        var emulator = Loader.EnterLoadCommand(ComputerType.Spectrum128K);

        emulator.IsFloatingBusEnabled = true;
        emulator.TapeManager.TapeLoadSpeed = TapeSpeed.Instant;

        var testFile = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/halt2int128.tap");
        emulator.TapeManager.InsertTape(testFile);

        emulator.RunFrames(110);

        SaveOutput(emulator.ScreenBuffer.FrameBuffer, "Halt2Int128k.png");

        var hash = emulator.ScreenBuffer.FrameBuffer.CalculateHash();
        hash.ShouldBe("936A455CE110BBD8316E7B94AD3416F37F70357E9E69737DC8C34893F9C27FC0");
    }
}
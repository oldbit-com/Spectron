using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Integration.Tests.Fixtures;

namespace OldBit.Spectron.Integration.Tests;

[Trait("Category", "Integration")]
[MeasureTime]
public class BorderTimingTests : IntegrationTestBase
{
    [Fact]
    public void RunBorderTest48K()
    {
        var emulator = Loader.EnterLoadCommand(ComputerType.Spectrum48K);

        emulator.IsFloatingBusEnabled = true;
        emulator.TapeManager.TapeLoadSpeed = TapeSpeed.Instant;

        var testFile = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/btime.tap");
        emulator.TapeManager.InsertTape(testFile);

        emulator.RunFrames(50);

        SaveOutput(emulator.ScreenBuffer.FrameBuffer, "BorderTiming48k.png");

        var hash = emulator.ScreenBuffer.FrameBuffer.CalculateHash();
        hash.ShouldBe("783B125E0901BCA1892DB2228FB7C1C4E95D25CC51AB99ED45121D3C3102E487");
    }
}
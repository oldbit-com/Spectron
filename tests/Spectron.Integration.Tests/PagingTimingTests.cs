using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Integration.Tests.Fixtures;

namespace OldBit.Spectron.Integration.Tests;

[Trait("Category", "Integration")]
[MeasureTime]
public class PagingTimingTests : IntegrationTestBase
{
    [Fact]
    public void PagingTiming_128K()
    {
        var emulator = Loader.EnterLoadCommand(ComputerType.Spectrum128K);

        emulator.IsFloatingBusEnabled = true;
        emulator.TapeManager.TapeLoadSpeed = TapeSpeed.Instant;

        var testFile = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/ptime.tap");
        emulator.TapeManager.InsertTape(testFile);

        emulator.RunFrames(45);

        SaveOutput(emulator.ScreenBuffer.FrameBuffer, "PagingTiming128k.png");

        var hash = emulator.ScreenBuffer.FrameBuffer.CalculateHash();
        hash.ShouldBe("1C76248ADA532891944834280E4B35E3E2249D22645668FDF578EB8A6DF8980E");
    }
}
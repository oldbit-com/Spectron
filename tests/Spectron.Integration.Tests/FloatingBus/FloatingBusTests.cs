using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Integration.Tests.Fixtures;

namespace OldBit.Spectron.Integration.Tests.FloatingBus;

[Trait("Category", "Integration")]
public class FloatingBusTests : IntegrationTestBase
{
    [Fact]
    public void RunFloat48K()
    {
        var emulator = Loader.EnterLoadCommand(ComputerType.Spectrum48K);

        emulator.IsFloatingBusEnabled = true;
        emulator.TapeManager.TapeLoadSpeed = TapeSpeed.Instant;

        var testFile = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/Float48k.tap");
        emulator.TapeManager.InsertTape(testFile);

        emulator.RunFrames(250);

        SaveOutput(emulator.ScreenBuffer.FrameBuffer, "Float48k.png");

        var hash = emulator.ScreenBuffer.FrameBuffer.CalculateHash();
        hash.ShouldBe("2072785D0E6C333359E1253AA8D5D9A5AACFE6FBA04AB8472AA54D8ED289F196");
    }

    [Fact]
    public void RunFloat128K()
    {
        var emulator = Loader.EnterLoadCommand(ComputerType.Spectrum128K);

        emulator.IsFloatingBusEnabled = true;
        emulator.TapeManager.TapeLoadSpeed = TapeSpeed.Instant;

        var testFile = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/Float128k.tap");
        emulator.TapeManager.InsertTape(testFile);

        emulator.RunFrames(550);

        SaveOutput(emulator.ScreenBuffer.FrameBuffer, "Float128k.png");

        var hash = emulator.ScreenBuffer.FrameBuffer.CalculateHash();
        hash.ShouldBe("E879D0B3115E3618592717849F46F84FC2420BD2F3C3E363CDE92F7D8DEF7FA2");
    }
}
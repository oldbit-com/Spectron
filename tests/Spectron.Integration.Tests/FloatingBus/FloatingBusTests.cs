using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Integration.Tests.Fixtures;

namespace OldBit.Spectron.Integration.Tests.FloatingBus;

[Trait("Category", "Integration")]
[MeasureTime]
public class FloatingBusTests: IntegrationTestBase
{
    [Fact]
    public void RunFloat_48K()
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
    public void RunFloat_128K()
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

    [Fact]
    public void RunFloatSpy_48K()
    {
        var emulator = Loader.EnterLoadCommand(ComputerType.Spectrum48K);

        emulator.IsFloatingBusEnabled = true;
        emulator.TapeManager.TapeLoadSpeed = TapeSpeed.Instant;

        var testFile = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/floatspy.tap");
        emulator.TapeManager.InsertTape(testFile);

        // Press 'T' to start the test
        emulator.RunFrames(75);
        emulator.KeyboardState.KeyDown([SpectrumKey.T]);
        emulator.RunFrames(50);
        emulator.KeyboardState.KeyUp([SpectrumKey.T]);

        emulator.RunFrames(14100);

        SaveOutput(emulator.ScreenBuffer.FrameBuffer, "FloatSpy48k.png");

        var hash = emulator.ScreenBuffer.FrameBuffer.CalculateHash();
        hash.ShouldBe("192AC740FD5293453A2159882649BBFA8BF49AD6A102C0CA2A9E080212A8D4A5");
    }

    [Fact]
    public void RunFloatSpy_128K()
    {
        var emulator = Loader.EnterLoadCommand(ComputerType.Spectrum128K);

        emulator.IsFloatingBusEnabled = true;
        emulator.TapeManager.TapeLoadSpeed = TapeSpeed.Instant;

        var testFile = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles/floatspy.tap");
        emulator.TapeManager.InsertTape(testFile);

        // Press 'T' to start the test
        emulator.RunFrames(75);
        emulator.KeyboardState.KeyDown([SpectrumKey.T]);
        emulator.RunFrames(100);
        emulator.KeyboardState.KeyUp([SpectrumKey.T]);

        emulator.RunFrames(15900);

        SaveOutput(emulator.ScreenBuffer.FrameBuffer, "FloatSpy128k.png");

        var hash = emulator.ScreenBuffer.FrameBuffer.CalculateHash();
        hash.ShouldBe("CD4D0CEC68D3F0802D2466207EAE0ADB34B08BEDC69C6AF7E1CE4089AD3C7B7E");
    }
}
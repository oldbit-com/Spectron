using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulator.Tests.Devices;

public class UlaTimexTests
{
    private const int ControlPort = 0xFF;

    private readonly UlaTimex _ulaTimex;

    public UlaTimexTests()
    {
        var keyboardState = new KeyboardState();

        var memory = new Memory128K(RomReader.ReadRom(RomType.Original128Bank0), RomReader.ReadRom(RomType.Original128Bank0));

        var screenBuffer = new ScreenBuffer(
            Hardware.Timex2048,
            memory,
            new UlaPlus());

        var z80 = new Z80(memory);

        _ulaTimex = new UlaTimex(keyboardState, screenBuffer, z80, new TapeManager());
    }

    [Fact]
    public void WriteControlPort_ShouldSetScreenModes()
    {
        _ulaTimex.WritePort(ControlPort, 0b000);
        _ulaTimex.ScreenMode.ShouldBe(ScreenMode.Spectrum);

        _ulaTimex.WritePort(ControlPort, 0b001);
        _ulaTimex.ScreenMode.ShouldBe(ScreenMode.TimexSecondScreen);

        _ulaTimex.WritePort(ControlPort, 0b010);
        _ulaTimex.ScreenMode.ShouldBe(ScreenMode.TimexHiColor);

        _ulaTimex.WritePort(ControlPort, 0b011);
        _ulaTimex.ScreenMode.ShouldBe(ScreenMode.TimexHiColorAlt);

        _ulaTimex.WritePort(ControlPort, 0b100);
        _ulaTimex.ScreenMode.ShouldBe(ScreenMode.TimexHiResAttr);

        _ulaTimex.WritePort(ControlPort, 0b101);
        _ulaTimex.ScreenMode.ShouldBe(ScreenMode.TimexHiResAttrAlt);

        _ulaTimex.WritePort(ControlPort, 0b110);
        _ulaTimex.ScreenMode.ShouldBe(ScreenMode.TimexHiRes);

        _ulaTimex.WritePort(ControlPort, 0b111);
        _ulaTimex.ScreenMode.ShouldBe(ScreenMode.TimexHiResDouble);
    }

    [Fact]
    public void WriteControlPort_ShouldInvokeScreenModeChangedEvent()
    {
        var eventRaised = false;

        _ulaTimex.ScreenModeChanged += (_, _) =>
        {
            _ulaTimex.ScreenMode.ShouldBe(ScreenMode.TimexSecondScreen);
            eventRaised = true;
        };

        _ulaTimex.WritePort(ControlPort, 0b001);

        _ulaTimex.ScreenMode.ShouldBe(ScreenMode.TimexSecondScreen);
        eventRaised.ShouldBeTrue();
    }

    [Fact]
    public void WriteControlPort_ShouldSetCorrectInkAndPaperColors()
    {
        _ulaTimex.WritePort(ControlPort, 0b000_000);
        _ulaTimex.Ink.ShouldBe(SpectrumPalette.Black);
        _ulaTimex.Paper.ShouldBe(SpectrumPalette.BrightWhite);

        _ulaTimex.WritePort(ControlPort, 0b001_000);
        _ulaTimex.Ink.ShouldBe(SpectrumPalette.BrightBlue);
        _ulaTimex.Paper.ShouldBe(SpectrumPalette.BrightYellow);

        _ulaTimex.WritePort(ControlPort, 0b010_000);
        _ulaTimex.Ink.ShouldBe(SpectrumPalette.BrightRed);
        _ulaTimex.Paper.ShouldBe(SpectrumPalette.BrightCyan);

        _ulaTimex.WritePort(ControlPort, 0b011_000);
        _ulaTimex.Ink.ShouldBe(SpectrumPalette.BrightMagenta);
        _ulaTimex.Paper.ShouldBe(SpectrumPalette.BrightGreen);

        _ulaTimex.WritePort(ControlPort, 0b100_000);
        _ulaTimex.Ink.ShouldBe(SpectrumPalette.BrightGreen);
        _ulaTimex.Paper.ShouldBe(SpectrumPalette.BrightMagenta);

        _ulaTimex.WritePort(ControlPort, 0b101_000);
        _ulaTimex.Ink.ShouldBe(SpectrumPalette.BrightCyan);
        _ulaTimex.Paper.ShouldBe(SpectrumPalette.BrightRed);

        _ulaTimex.WritePort(ControlPort, 0b110_000);
        _ulaTimex.Ink.ShouldBe(SpectrumPalette.BrightYellow);
        _ulaTimex.Paper.ShouldBe(SpectrumPalette.BrightBlue);

        _ulaTimex.WritePort(ControlPort, 0b111_000);
        _ulaTimex.Ink.ShouldBe(SpectrumPalette.BrightWhite);
        _ulaTimex.Paper.ShouldBe(SpectrumPalette.Black);
    }

    [Fact]
    public void ReadNonUlaPort_ShouldReturnNull()
    {
        var value = _ulaTimex.ReadPort(0xAA);
        value.ShouldBeNull();
    }

    [Fact]
    public void WriteControlPort_ShouldUpdateLastControlValue()
    {
        _ulaTimex.WritePort(ControlPort, 0x55);

        var value = _ulaTimex.ReadPort(ControlPort);
        value.ShouldBe((byte)0x55);

        _ulaTimex.WritePort(ControlPort, 0xAA);

        value = _ulaTimex.ReadPort(ControlPort);
        value.ShouldBe((byte)0xAA);
    }
}

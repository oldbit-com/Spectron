using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices;

internal sealed class UlaTimex(
    KeyboardState keyboardState,
    ScreenBuffer screenBuffer,
    EmulatorClock clock,
    Z80 cpu,
    TapeManager tapeManager) : Ula(keyboardState, screenBuffer, clock, cpu, tapeManager)
{
    private byte _lastControlValue;

    internal const int ControlPort = 0xFF;
    internal ScreenMode ScreenMode { get; private set; }
    internal Color Paper { get; private set; }
    internal Color Ink { get; private set; }
    internal byte PaperIndex { get; private set; } = 0x07;

    internal event EventHandler<EventArgs>? ScreenModeChanged;

    public override byte? ReadPort(Word address)
    {
        var result = base.ReadPort(address);

        if (result != null)
        {
            return result;
        }

        if ((address & 0xFF) != ControlPort)
        {
            return null;
        }

        return _lastControlValue;
    }

    public override void WritePort(Word address, byte value)
    {
        base.WritePort(address, value);

        if ((address & 0xFF) != ControlPort)
        {
            return;
        }

        ScreenMode = (value & 0b111) switch
        {
            0b000 => ScreenMode.Spectrum,
            0b001 => ScreenMode.TimexSecondScreen,
            0b010 => ScreenMode.TimexHiColor,
            0b011 => ScreenMode.TimexHiColorAlt,
            0b100 => ScreenMode.TimexHiResAttr,
            0b101 => ScreenMode.TimexHiResAttrAlt,
            0b110 => ScreenMode.TimexHiRes,
            0b111 => ScreenMode.TimexHiResDouble,
            _ => ScreenMode
        };

        Ink = (value & 0b111_000) switch
        {
            0b000_000 => SpectrumPalette.Black,             // 000
            0b001_000 => SpectrumPalette.BrightBlue,        // 001
            0b010_000 => SpectrumPalette.BrightRed,         // 010
            0b011_000 => SpectrumPalette.BrightMagenta,     // 011
            0b100_000 => SpectrumPalette.BrightGreen,       // 100
            0b101_000 => SpectrumPalette.BrightCyan,        // 101
            0b110_000 => SpectrumPalette.BrightYellow,      // 110
            0b111_000 => SpectrumPalette.BrightWhite,       // 111
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

        Paper = (value & 0b111_000) switch
        {
            0b000_000 => SpectrumPalette.BrightWhite,       // 111
            0b001_000 => SpectrumPalette.BrightYellow,      // 110
            0b010_000 => SpectrumPalette.BrightCyan,        // 101
            0b011_000 => SpectrumPalette.BrightGreen,       // 100
            0b100_000 => SpectrumPalette.BrightMagenta,     // 011
            0b101_000 => SpectrumPalette.BrightRed,         // 010
            0b110_000 => SpectrumPalette.BrightBlue,        // 001
            0b111_000 => SpectrumPalette.Black,             // 000
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

        PaperIndex = (byte)(~(value >> 3) & 0x07);          // Inverted to match the paper color index

        if (_lastControlValue != value)
        {
            ScreenModeChanged?.Invoke(this, EventArgs.Empty);
        }

        _lastControlValue = value;
    }

    internal override bool IsUlaPort(Word address) => (address & 0xFF) == 0xFE;

    internal override void Reset() => WritePort(ControlPort, 0);
}
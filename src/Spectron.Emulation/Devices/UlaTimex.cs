using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulation.Devices;

public sealed class UlaTimex : IDevice
{
    private byte _lastControlValue;

    internal const int ControlPort = 0xFF;
    internal ScreenMode ScreenMode { get; private set; }
    internal Color Paper { get; private set; }
    internal Color Ink { get; private set; }

    internal event EventHandler<EventArgs>? ScreenModeChanged;

    public byte? ReadPort(Word address)
    {
        if ((address & 0xFF) != ControlPort)
        {
            return null;
        }

        return _lastControlValue;
    }

    public void WritePort(Word address, byte value)
    {
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
            0b110 => ScreenMode.TimexHiRes,
            0b111 => ScreenMode.TimexHiResDouble,
            // TODO: Handle alternate modes
            _ => ScreenMode
        };

        Ink = (value & 0b111_000) switch
        {
            0b000_000 => SpectrumPalette.Black,
            0b001_000 => SpectrumPalette.BrightBlue,
            0b010_000 => SpectrumPalette.BrightRed,
            0b011_000 => SpectrumPalette.BrightMagenta,
            0b100_000 => SpectrumPalette.BrightGreen,
            0b101_000 => SpectrumPalette.BrightCyan,
            0b110_000 => SpectrumPalette.BrightYellow,
            0b111_000 => SpectrumPalette.BrightWhite,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

        Paper = (value & 0b111_000) switch
        {
            0b000_000 => SpectrumPalette.BrightWhite,
            0b001_000 => SpectrumPalette.BrightYellow,
            0b010_000 => SpectrumPalette.BrightCyan,
            0b011_000 => SpectrumPalette.BrightGreen,
            0b100_000 => SpectrumPalette.BrightMagenta,
            0b101_000 => SpectrumPalette.BrightRed,
            0b110_000 => SpectrumPalette.BrightBlue,
            0b111_000 => SpectrumPalette.Black,
            _ => throw new ArgumentOutOfRangeException(nameof(value))
        };

        if (_lastControlValue != value)
        {
            ScreenModeChanged?.Invoke(this, EventArgs.Empty);
        }

        _lastControlValue = value;
    }

    internal void Reset() => WritePort(ControlPort, 0);
}
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulation.Devices.Timex;

public sealed class TimexControl : IDevice
{
    private byte _lastValue;

    internal ScreenMode ScreenMode { get; private set; }
    internal Color Paper { get; private set; }
    internal Color Ink { get; private set; }

    internal event EventHandler<EventArgs>? ScreenModeChanged;

    public byte? ReadPort(Word address)
    {
        if ((address & 0xFF) != 0xFF)
        {
            return null;
        }

        return _lastValue;
    }

    public void WritePort(Word address, byte value)
    {
        if ((address & 0xFF) != 0xFF)
        {
            return;
        }

        ScreenMode = (value & 0b111) switch
        {
            0b000 => ScreenMode.Spectrum,
            0b001 => ScreenMode.TimexScreen1,
            0b010 => ScreenMode.TimexHiColor,
            0b011 => ScreenMode.TimexHiColorAlt,
            0b110 => ScreenMode.TimexHiRes,
            // TODO: Handle alternate modes
            _ => ScreenMode
        };

        Ink = (value & 0b111_000) switch
        {
            0b000_000 => SpectrumPalette.Black,
            0b001_000 => SpectrumPalette.Blue,
            0b010_000 => SpectrumPalette.Red,
            0b011_000 => SpectrumPalette.Magenta,
            0b100_000 => SpectrumPalette.Green,
            0b101_000 => SpectrumPalette.Cyan,
            0b110_000 => SpectrumPalette.Yellow,
            0b111_000 => SpectrumPalette.White
        };

        Paper = (value & 0b111_000) switch
        {
            0b000_000 => SpectrumPalette.White,
            0b001_000 => SpectrumPalette.Yellow,
            0b010_000 => SpectrumPalette.Cyan,
            0b011_000 => SpectrumPalette.Green,
            0b100_000 => SpectrumPalette.Magenta,
            0b101_000 => SpectrumPalette.Red,
            0b110_000 => SpectrumPalette.Blue,
            0b111_000 => SpectrumPalette.Black
        };

        if (_lastValue != value)
        {
            ScreenModeChanged?.Invoke(this, EventArgs.Empty);
        }

        _lastValue = value;
    }
}
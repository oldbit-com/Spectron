using OldBit.Spectral.Emulation.Screen;

namespace OldBit.Spectral.Emulation.Devices;

/// <summary>
/// Handles the ULA+ device that extends the ZX Spectrum's color capabilities.
/// https://zxdesign.itch.io/ulaplus
/// </summary>
internal sealed class UlaPlus : IDevice
{
    private enum Register { PaletteGroup, ModeGroup }
    private const int RegisterPort = 0xBF3B;
    private const int DataPort = 0xFF3B;

    private readonly Color[][] _paletteColors = [new Color[16], new Color[16], new Color[16], new Color[16]];
    private Register _register;

    private int _paletteGroup;
    private byte _lastWrittenValue;

    internal bool IsActive { get; private set; }
    internal bool IsEnabled { get; set; }

    internal delegate void ActiveChangedEvent(EventArgs e);
    internal event ActiveChangedEvent? ActiveChanged;

    public void WritePort(Word address, byte value)
    {
        switch (address)
        {
            case RegisterPort:
                _register = (value & 0x40) == 0 ? Register.PaletteGroup : Register.ModeGroup;

                if (_register == Register.PaletteGroup)
                {
                    _paletteGroup = value & 0x3F;
                }
                break;

            case DataPort:
                _lastWrittenValue = value;

                switch (_register)
                {
                    case Register.PaletteGroup:
                        var paletteIndex = _paletteGroup >> 4;
                        var colorIndex = _paletteGroup & 0x0F;
                        var color = TranslateColor(value);

                        _paletteColors[paletteIndex][colorIndex] = color;

                        break;

                    case Register.ModeGroup:
                        IsActive = (value & 0x01) == 0x01;
                        ActiveChanged?.Invoke(EventArgs.Empty);

                        break;
                }
                break;
        }
    }

    public byte? ReadPort(Word address)
    {
        if (address != DataPort)
        {
            return null;
        }

        return _lastWrittenValue;
    }

    internal Color GetInkColor(byte attribute)
    {
        var paletteIndex = attribute >> 6;
        var palette = _paletteColors[paletteIndex];
        var colorIndex = attribute & 0x07;

        return palette[colorIndex];
    }

    internal Color GetPaperColor(byte attribute)
    {
        var paletteIndex = attribute >> 6;
        var palette = _paletteColors[paletteIndex];
        var colorIndex = ((attribute >> 3) & 0x07) | 8;

        return palette[colorIndex];
    }

    private static Color TranslateColor(int value)
    {
        var green = (value & 0b11100000) >> 5;
        green = Scale3BitsColor(green);

        var red = (value & 0b00011100) >> 2;
        red = Scale3BitsColor(red);

        var blue = (value & 0b00000011) << 1;
        blue = blue == 0 ? blue : blue | 0x01;
        blue = Scale3BitsColor(blue);

        return new Color(red, green, blue);
    }

    private static byte Scale3BitsColor(int color) => (byte)(color << 5 | color << 2 | color & 0x03);
}
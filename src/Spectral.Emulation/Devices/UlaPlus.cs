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
    private bool _isActive;
    private byte _lastWrittenValue;

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
                        _isActive = (value & 0x01) == 0x01;
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

    public Color GetInkColor(byte attribute)
    {
        var paletteIndex = attribute >> 6;
        var palette = _paletteColors[paletteIndex];
        var colorIndex = attribute & 0x07;

        return palette[colorIndex];
    }

    public Color GetPaperColor(byte attribute)
    {
        var paletteIndex = attribute >> 6;
        var palette = _paletteColors[paletteIndex];
        var colorIndex = attribute >> 3 & 0x07 | 8;

        return palette[colorIndex];
    }

    private static Color TranslateColor(int value)
    {
        var green = value & 0b11100000 >> 5;
        green = ScaleToByte(green);

        var red = value & 0b00011100 >> 2;
        red = ScaleToByte(red);

        var blue = value & 0b00000011;
        blue = blue == 0 ? blue : blue | 1; // Missing lowest bit is set to 1 if any other 2 bits are set

        return new Color(red, green, blue);
    }

    // When scaling 3-bits of color data to more bits for emulators that operate in high color mode,
    // simply concatenate the bits repeatedly and then truncate to as many bits as needed.
    private static byte ScaleToByte(int color) => (byte)(color << 5 | color << 2 | color >> 1);
}
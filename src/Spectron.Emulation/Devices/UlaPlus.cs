using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulation.Devices;

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
    private bool _isActive;
    private byte _lastWrittenValue;

    internal byte PaletteGroup { get; set; }
    internal bool IsEnabled { get; set; }
    internal bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            ActiveChanged?.Invoke(EventArgs.Empty);
        }
    }

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
                    PaletteGroup = (byte)(value & 0x3F);
                }
                break;

            case DataPort:
                _lastWrittenValue = value;

                switch (_register)
                {
                    case Register.PaletteGroup:
                        var paletteIndex = PaletteGroup >> 4;
                        var colorIndex = PaletteGroup & 0x0F;
                        var color = ColorFromValue(value);

                        _paletteColors[paletteIndex][colorIndex] = color;
                        break;

                    case Register.ModeGroup:
                        IsActive = (value & 0x01) == 0x01;
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

    internal void Reset()
    {
        IsActive = false;

        PaletteGroup = 0;
        _register = Register.PaletteGroup;
        _lastWrittenValue = 0;

        foreach (var palette in _paletteColors)
        {
            Array.Fill(palette, SpectrumPalette.White);
        }
    }

    internal byte[] GetPaletteData()
    {
        var data = new byte[64];

        for (var i = 0; i < _paletteColors.Length; i++)
        {
            for (var j = 0; j < _paletteColors[i].Length; j++)
            {
                var color = _paletteColors[i][j];
                var value = ValueFromColor(color);

                data[i * 16 + j] = value;
            }
        }

        return data;
    }

    internal void SetPaletteData(byte[] data)
    {
        for (var i = 0; i < _paletteColors.Length; i++)
        {
            for (var j = 0; j < _paletteColors[i].Length; j++)
            {
                var value = data[i * 16 + j];
                var color = ColorFromValue(value);

                _paletteColors[i][j] = color;
            }
        }
    }

    internal static Color ColorFromValue(int value)
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

    private static byte ValueFromColor(Color color)
    {
        var red = color.Red >> 5;
        var green = color.Green >> 5;
        var blue = color.Blue >> 5;

        return (byte)((green << 5) | (red << 2) | (blue >> 1));
    }

    private static byte Scale3BitsColor(int color) => (byte)(color << 5 | color << 2 | color & 0x03);
}
namespace OldBit.ZXSpectrum.Emulator.Screen;

public class ScreenRenderer
{
    private readonly Border _border;
    internal const int BorderTop = 64;              // top border height
    internal const int BorderBottom = 56;           // bottom border height
    internal const int BorderLeft = 48;             // left border width
    internal const int BorderRight = 48;            // right border width

    internal const int ScreenWidth = 256;           // screen width, excluding borders
    internal const int ScreenHeight = 192;          // screen height, excluding borders

    internal const int CyclesPerLine = 224;         // T-states per line

    private const int BytesPerPixel = 4;

    public const int TotalWidth = BorderLeft + ScreenWidth + BorderRight;           // total screen width with borders
    public const int TotalHeight = BorderTop + ScreenHeight + BorderBottom;         // total screen height with borders

    private readonly byte[] _pixelData = new byte[TotalHeight * TotalWidth * BytesPerPixel];
    private readonly int[] _clockCycleAtPixel = new int[TotalHeight * TotalWidth];
    private readonly byte[] _bits = [0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01];
    private readonly Color[] _paperColors = new Color[256];
    private readonly Color[] _inkColors = new Color[256];

    private int _currentFrame = 1;

    public ScreenRenderer(Border border)
    {
        _border = border;

        for (var line = 0; line < TotalHeight; line++)
        {
            // Pre-calculate the clock cycle for each pixel
            for (var pixel = 0; pixel < TotalWidth; pixel++)
            {
                // 14366 is the first pixel of the first line of the screen
                _clockCycleAtPixel[line * TotalWidth + pixel] = (64 + line) * CyclesPerLine + (48 + pixel) / 2;
            }

            // Pre-calculate the paper and ink colors for each attribute
            for (var i = 0; i < 256; i++)
            {
                _paperColors[i] = Colors.PaperColors[i & 0b01111000];
                _inkColors[i] = Colors.InkColors[i & 0b01000111];
            }
        }
    }

    public byte[] Render(ReadOnlySpan<byte> screen)
    {
        // Top border
        for (var pixel = 0; pixel < TotalWidth * BorderTop * BytesPerPixel; pixel += BytesPerPixel)
        {
            SetBorderPixelColor(pixel);
        }

        // Main screen and left/right border
        for (var line = 0; line < ScreenLineAddresses.Length; line++)
        {
            // Left border
            var leftBorderPixel = TotalWidth * (BorderTop + line) * BytesPerPixel;
            for (var pixel = leftBorderPixel; pixel < leftBorderPixel + BorderLeft * BytesPerPixel; pixel += BytesPerPixel)
            {
                SetBorderPixelColor(pixel);
            }

            // Screen content
            var contentPixel =  (TotalWidth * (BorderTop + line) + BorderLeft) * BytesPerPixel;
            var address = ScreenLineAddresses[line] - 0x4000;
            for (var column = 0; column < 32; column++)
            {
                var attribute = screen[0x1800 + 32 * (line / 8) + column];
                var value = screen[address + column];

                var flashBitSet = (attribute & 0x80) != 0 && _currentFrame >= 32;
                var paperColor = _paperColors[attribute];
                var inkColor = _inkColors[attribute];

                foreach (var pixelBit in _bits)
                {
                    var pixelSet = (value & pixelBit) != 0;
                    var color = pixelSet != flashBitSet ? inkColor : paperColor;

                    SetPixelColor(contentPixel, color);

                    contentPixel += BytesPerPixel;
                }
            }

            // Right border
            var rightBorderPixel = leftBorderPixel + (BorderLeft + ScreenWidth) * BytesPerPixel;
            for (var pixel = rightBorderPixel; pixel < rightBorderPixel + BorderRight * BytesPerPixel; pixel += BytesPerPixel)
            {
                SetBorderPixelColor(pixel);
            }
        }

        // Bottom border
        var startBottomBorderPixel = TotalWidth * (BorderTop + ScreenHeight) * BytesPerPixel;
        for (var pixel = startBottomBorderPixel; pixel < startBottomBorderPixel + TotalWidth * BorderBottom * BytesPerPixel; pixel += BytesPerPixel)
        {
            SetBorderPixelColor(pixel);
        }

        ResetBorderStates();
        UpdateFrameCounter();

        return _pixelData;
    }

    private void SetBorderPixelColor(int pixel)
    {
        var pixelCycle = _clockCycleAtPixel[pixel / BytesPerPixel];
        var borderColor = _border.GetBorderColor(pixelCycle);

        SetPixelColor(pixel, borderColor);
    }

    private void SetPixelColor(int pixel, Color color)
    {
        _pixelData[pixel] = color.Red;
        _pixelData[pixel + 1] = color.Green;
        _pixelData[pixel + 2] = color.Blue;
        _pixelData[pixel + 3] = 255;
    }

    private void UpdateFrameCounter()
    {
        _currentFrame += 1;
        if (_currentFrame > 50)
        {
            _currentFrame = 1;
        }
    }

    private void ResetBorderStates()
    {
        _border.Reset();
    }

    private static readonly Word[] ScreenLineAddresses = [
        0x4000, 0x4100, 0x4200, 0x4300, 0x4400, 0x4500, 0x4600, 0x4700, // Lines 0-7
        0x4020, 0x4120, 0x4220, 0x4320, 0x4420, 0x4520, 0x4620, 0x4720, // Lines 8-15
        0x4040, 0x4140, 0x4240, 0x4340, 0x4440, 0x4540, 0x4640, 0x4740, // Lines 16-23
        0x4060, 0x4160, 0x4260, 0x4360, 0x4460, 0x4560, 0x4660, 0x4760, // Lines 24-31
        0x4080, 0x4180, 0x4280, 0x4380, 0x4480, 0x4580, 0x4680, 0x4780, // Lines 32-39
        0x40A0, 0x41A0, 0x42A0, 0x43A0, 0x44A0, 0x45A0, 0x46A0, 0x47A0, // Lines 40-47
        0x40C0, 0x41C0, 0x42C0, 0x43C0, 0x44C0, 0x45C0, 0x46C0, 0x47C0, // Lines 48-55
        0x40E0, 0x41E0, 0x42E0, 0x43E0, 0x44E0, 0x45E0, 0x46E0, 0x47E0, // Lines 56-63
        0x4800, 0x4900, 0x4A00, 0x4B00, 0x4C00, 0x4D00, 0x4E00, 0x4F00, // Lines 64-71
        0x4820, 0x4920, 0x4A20, 0x4B20, 0x4C20, 0x4D20, 0x4E20, 0x4F20, // Lines 72-79
        0x4840, 0x4940, 0x4A40, 0x4B40, 0x4C40, 0x4D40, 0x4E40, 0x4F40, // Lines 80-87
        0x4860, 0x4960, 0x4A60, 0x4B60, 0x4C60, 0x4D60, 0x4E60, 0x4F60, // Lines 88-95
        0x4880, 0x4980, 0x4A80, 0x4B80, 0x4C80, 0x4D80, 0x4E80, 0x4F80, // Lines 96-103
        0x48A0, 0x49A0, 0x4AA0, 0x4BA0, 0x4CA0, 0x4DA0, 0x4EA0, 0x4FA0, // Lines 104-111
        0x48C0, 0x49C0, 0x4AC0, 0x4BC0, 0x4CC0, 0x4DC0, 0x4EC0, 0x4FC0, // Lines 112-119
        0x48E0, 0x49E0, 0x4AE0, 0x4BE0, 0x4CE0, 0x4DE0, 0x4EE0, 0x4FE0, // Lines 120-127
        0x5000, 0x5100, 0x5200, 0x5300, 0x5400, 0x5500, 0x5600, 0x5700, // Lines 128-135
        0x5020, 0x5120, 0x5220, 0x5320, 0x5420, 0x5520, 0x5620, 0x5720, // Lines 136-143
        0x5040, 0x5140, 0x5240, 0x5340, 0x5440, 0x5540, 0x5640, 0x5740, // Lines 144-151
        0x5060, 0x5160, 0x5260, 0x5360, 0x5460, 0x5560, 0x5660, 0x5760, // Lines 152-159
        0x5080, 0x5180, 0x5280, 0x5380, 0x5480, 0x5580, 0x5680, 0x5780, // Lines 160-167
        0x50A0, 0x51A0, 0x52A0, 0x53A0, 0x54A0, 0x55A0, 0x56A0, 0x57A0, // Lines 168-175
        0x50C0, 0x51C0, 0x52C0, 0x53C0, 0x54C0, 0x55C0, 0x56C0, 0x57C0, // Lines 176-183
        0x50E0, 0x51E0, 0x52E0, 0x53E0, 0x54E0, 0x55E0, 0x56E0, 0x57E0, // Lines 184-191
    ];
}
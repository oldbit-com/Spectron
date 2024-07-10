using OldBit.ZXSpectrum.Emulator.Hardware;

namespace OldBit.ZXSpectrum.Emulator.Screen;

public class ContentRenderer(ScreenBuffer screenBuffer, Memory48K memory)
{
    private readonly bool[] _needsRefresh = new bool[32*24*8];

    private int _lastTicks = Constants.FirstDataPixelTick;
    private int _frameCount = 1;
    private bool _isFlashOnFrame;

    public void Update(int ticks)
    {
        if (ticks >= FastLookup.PixelTicks.Length)
        {
            return;
        }

        for (var screenTick = _lastTicks; screenTick <= ticks; screenTick++)
        {
            var pixelData = FastLookup.PixelTicks[screenTick];
            if (pixelData is null)
            {
                continue;
            }

            if (!_needsRefresh[pixelData.Address])
            {
                continue;
            }

            var screenData = memory.Screen[pixelData.Address];
            var attrValue = memory.Screen[pixelData.AttrAddress];
            var colors = FastLookup.AttributeColors[attrValue];

            var isFlashOn = colors.IsFlashOn && _isFlashOnFrame;

            for (var i = 0; i < FastLookup.BitMasks.Length; i++)
            {
                var color = (screenData & FastLookup.BitMasks[i]) != 0 ^ isFlashOn ? colors.Ink : colors.Paper;
                screenBuffer.Data[pixelData.BufferAddress + i] = color;
            }

            _needsRefresh[pixelData.Address] = false;
        }

        _lastTicks = ticks + 1;
    }

    public void NewFrame()
    {
        _lastTicks = Constants.FirstDataPixelTick;
        _frameCount += 1;

        if (_frameCount >= 32)
        {
            ToggleFlash();
            _frameCount = 1;
        }
    }

    public void UpdateScreen(Word address) => UpdateScreenPrivate(address - 0x4000);

    private void UpdateScreenPrivate(int address)
    {
        if (address < 0x1800)
        {
            // Screen byte
            _needsRefresh[address] = true;
        }
        else
        {
            // Attribute byte affecting 8 screen bytes
            var screenAddress = FastLookup.LineAddressForAttrAddress[address - 0x1800];
            _needsRefresh[screenAddress] = true;
            _needsRefresh[screenAddress + 256] = true;
            _needsRefresh[screenAddress + 512] = true;
            _needsRefresh[screenAddress + 768] = true;
            _needsRefresh[screenAddress + 1024] = true;
            _needsRefresh[screenAddress + 1280] = true;
            _needsRefresh[screenAddress + 1536] = true;
            _needsRefresh[screenAddress + 1792] = true;
        }
    }

    private void ToggleFlash()
    {
        _isFlashOnFrame = !_isFlashOnFrame;

        for (var attrAddress = 0x1800; attrAddress < 0x1B00; attrAddress++)
        {
            if ((memory.Screen[attrAddress] & 0x80) != 0)
            {
                UpdateScreenPrivate(attrAddress);
            }
        }
    }
}
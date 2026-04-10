using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen.Modes;

/// <summary>
/// Timex HiColor screen updater. Uses 0x4000 for data and 0x6000 for attribute data.
/// </summary>
internal class TimexHiColorScreenUpdater(
    FrameBuffer frameBuffer,
    IEmulatorMemory memory,
    bool isAlternate) : IScreenUpdater
{
    private const int SpectrumScreenAddress = 0x4000;
    private const int AttributesScreenAddress = 0x6000;

    private readonly bool[] _dirtyAddresses = new bool[32 * 24 * 8];
    private bool _isFlashOnFrame;

    public void Update(int frameBufferIndex, Word bitmapAddress, Word attributeAddress, int byteIndex)
    {
        if (!_dirtyAddresses[bitmapAddress])
        {
            return;
        }

        var bitmap = memory.Read((Word)(bitmapAddress + (isAlternate ? AttributesScreenAddress : SpectrumScreenAddress)));
        var attribute = memory.Read((Word)(bitmapAddress + AttributesScreenAddress));

        var attributeData = FastLookup.AttributeData[attribute];
        var isFlashOn = attributeData.IsFlashOn && _isFlashOnFrame;

        for (var bit = 0; bit < FastLookup.BitMasks.Length; bit++)
        {
            var color = (bitmap & FastLookup.BitMasks[bit]) != 0 ^ isFlashOn ? attributeData.Ink : attributeData.Paper;
            frameBuffer.Pixels[frameBufferIndex + bit] = color;
        }

        _dirtyAddresses[bitmapAddress] = false;
    }

    public void Invalidate() => Array.Fill(_dirtyAddresses, true);

    public void SetDirty(int address)
    {
        if (address >= AttributesScreenAddress)
        {
            // Attribute data
            _dirtyAddresses[address - AttributesScreenAddress] = true;
        }
        else
        {
            // Pixel data
            _dirtyAddresses[address - SpectrumScreenAddress] = true;
        }
    }

    public void ToggleFlash()
    {
        _isFlashOnFrame = !_isFlashOnFrame;

        for (var attrAddress = AttributesScreenAddress; attrAddress < AttributesScreenAddress + 0x1800; attrAddress++)
        {
            if ((memory.Read((Word)attrAddress) & 0x80) != 0)
            {
                SetDirty(attrAddress - 0x2000);
            }
        }
    }
}
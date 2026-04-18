using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen.Modes;

/// <summary>
/// Standard Spectrum screen updater. Also handles ULA+ coloring and Timex second screen.
/// </summary>
internal sealed class SpectrumScreenUpdater(
    FrameBuffer frameBuffer,
    IEmulatorMemory memory,
    UlaPlus ulaPlus) : IScreenUpdater
{
    private const int ScreenBaseAddress = 0x4000;

    private readonly bool[] _dirtyAddresses = new bool[32 * 24 * 8];
    private bool _isFlashOnFrame;

    public void Update(int frameBufferIndex, Word bitmapAddress, Word attributeAddress, int byteIndex)
    {
        if (!_dirtyAddresses[bitmapAddress])
        {
            return;
        }

        var bitmap = memory.ReadScreen(bitmapAddress);
        var attribute = memory.ReadScreen(attributeAddress);

        var attributeData = FastLookup.AttributeData[attribute];
        var isFlashOn = attributeData.IsFlashOn && _isFlashOnFrame;

        for (var bit = 0; bit < FastLookup.BitMasks.Length; bit++)
        {
            Color color;

            if (ulaPlus is { IsEnabled: true, IsActive: true })
            {
                color = (bitmap & FastLookup.BitMasks[bit]) != 0 ? ulaPlus.GetInkColor(attribute) : ulaPlus.GetPaperColor(attribute);
            }
            else
            {
                color = (bitmap & FastLookup.BitMasks[bit]) != 0 ^ isFlashOn ? attributeData.Ink : attributeData.Paper;
            }

            frameBuffer.Pixels[frameBufferIndex + bit] = color;
        }

        _dirtyAddresses[bitmapAddress] = false;
    }

    public void Invalidate() => Array.Fill(_dirtyAddresses, true);

    public void SetDirty(int address)
    {
        address -= ScreenBaseAddress;

        if (address < 0x1800)
        {
            // Single screen byte
            _dirtyAddresses[address] = true;
        }
        else
        {
            // Attribute byte affecting 8 screen bytes, unrolled for performance (~7x faster than a for loop)
            var screenAddress = FastLookup.LineAddressForAttrAddress[address - 0x1800];

            _dirtyAddresses[screenAddress] = true;
            _dirtyAddresses[screenAddress + 256] = true;
            _dirtyAddresses[screenAddress + 512] = true;
            _dirtyAddresses[screenAddress + 768] = true;
            _dirtyAddresses[screenAddress + 1024] = true;
            _dirtyAddresses[screenAddress + 1280] = true;
            _dirtyAddresses[screenAddress + 1536] = true;
            _dirtyAddresses[screenAddress + 1792] = true;
        }
    }

    public void ToggleFlash()
    {
        _isFlashOnFrame = !_isFlashOnFrame;

        const int startAttrAddress = 0x1800 + ScreenBaseAddress;
        const int endAttrAddress = 0x1B00 + ScreenBaseAddress;

        for (var attrAddress = startAttrAddress; attrAddress < endAttrAddress; attrAddress++)
        {
            if ((memory.Read((Word)attrAddress) & 0x80) != 0)
            {
                SetDirty(attrAddress);
            }
        }
    }
}
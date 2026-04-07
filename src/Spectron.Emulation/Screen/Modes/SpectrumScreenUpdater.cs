using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen.Modes;

internal sealed class SpectrumScreenUpdater(
    FrameBuffer frameBuffer,
    IEmulatorMemory memory,
    UlaPlus ulaPlus,
    Word screenBaseAddress) : IScreenUpdater
{
    private readonly bool[] _dirtyAddresses = new bool[32 * 24 * 8];
    private bool _isFlashOnFrame;

    public void Update(int frameBufferIndex, Word bitmapAddress, Word attributeAddress)
    {
        if (!_dirtyAddresses[bitmapAddress])
        {
            return;
        }

        var bitmap = memory.Read((Word)(bitmapAddress + screenBaseAddress));
        var attribute = memory.Read((Word)(attributeAddress + screenBaseAddress));

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
        address -= screenBaseAddress;

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

        var startAttrAddress = 0x1800 + screenBaseAddress;
        var endAttrAddress = 0x1B00 + screenBaseAddress;

        for (var attrAddress = startAttrAddress; attrAddress < endAttrAddress; attrAddress++)
        {
            if ((memory.Read((Word)attrAddress) & 0x80) != 0)
            {
                SetDirty(attrAddress);
            }
        }
    }
}
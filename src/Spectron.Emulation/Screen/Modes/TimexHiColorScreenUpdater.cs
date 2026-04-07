using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen.Modes;

internal class TimexHiColorScreenUpdater(
    FrameBuffer frameBuffer,
    IEmulatorMemory memory) : IScreenUpdater
{
    private readonly bool[] _dirtyAddresses = new bool[32 * 24 * 8];
    private bool _isFlashOnFrame;

    public void Update(int frameBufferIndex, Word bitmapAddress, Word attributeAddress)
    {
        if (!_dirtyAddresses[bitmapAddress])
        {
            return;
        }

        var bitmap = memory.Read((Word)(bitmapAddress + 0x4000));
        var attribute = memory.Read((Word)(bitmapAddress + 0x6000));

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
        _dirtyAddresses[address - 0x4000] = true;
    }

    public void ToggleFlash()
    {
        _isFlashOnFrame = !_isFlashOnFrame;

        const int startAttrAddress = 0x6000;
        const int endAttrAddress = 0x7800;

        for (var attrAddress = startAttrAddress; attrAddress < endAttrAddress; attrAddress++)
        {
            if ((memory.Read((Word)attrAddress) & 0x80) != 0)
            {
                SetDirty(attrAddress - 0x2000);
            }
        }
    }
}
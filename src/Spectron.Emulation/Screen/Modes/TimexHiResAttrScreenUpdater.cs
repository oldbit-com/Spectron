using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen.Modes;

/// <summary>
/// Timex undocumented HiRes screen updater. Uses standard screen and attributes for data.
/// </summary>
internal class TimexHiResAttrScreenUpdater(
    FrameBuffer frameBuffer,
    IEmulatorMemory memory,
    Color ink,
    Color paper,
    bool isAlternative = false) : IScreenUpdater
{
    private readonly bool[] _dirtyAddresses = new bool[16384];
    private readonly int _offset = isAlternative ? 0x2000 : 0;

    public void Update(int frameBufferIndex, Word bitmapAddress, Word attributeAddress, int byteIndex)
    {
        var actualBitmapAddress = (Word)(bitmapAddress + _offset);
        var actualAttributeAddress = (Word)(attributeAddress + _offset);

        if (byteIndex == 1)
        {
            Update(frameBufferIndex, bitmapAddress, actualBitmapAddress);
            Update(frameBufferIndex + 8, (Word)(bitmapAddress + 0x2000), actualAttributeAddress);
        }
        else
        {
           Update(frameBufferIndex + 8, bitmapAddress, actualBitmapAddress);
           Update(frameBufferIndex + 16, (Word)(bitmapAddress + 0x2000), actualAttributeAddress);
        }
    }

    private void Update(int frameBufferIndex, Word bitmapAddress, Word actualBitmapAddress)
    {
        if (!_dirtyAddresses[bitmapAddress])
        {
            return;
        }

        var bitmap = memory.Read((Word)(actualBitmapAddress + 0x4000));

        for (var bit = 0; bit < FastLookup.BitMasks.Length; bit++)
        {
            var color = (bitmap & FastLookup.BitMasks[bit]) != 0 ? ink : paper;
            frameBuffer.Pixels[frameBufferIndex + bit] = color;
        }

        _dirtyAddresses[bitmapAddress] = false;
    }

    public void Invalidate() => Array.Fill(_dirtyAddresses, true);

    public void SetDirty(int address)
    {
        address -= isAlternative ? 0x6000 : 0x4000;

        if (address < 0x1800)
        {
            // Single screen byte
            _dirtyAddresses[address] = true;
        }
        else
        {
            // Attribute byte affecting 8 screen bytes, unrolled for performance (~7x faster than a for loop)
            var screenAddress = FastLookup.LineAddressForAttrAddress[address - 0x1800] + 0x2000;

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
        // Not supported in this mode
    }
}
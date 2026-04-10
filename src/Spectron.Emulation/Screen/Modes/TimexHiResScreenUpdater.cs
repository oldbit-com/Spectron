using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen.Modes;

/// <summary>
/// Timex HiRes screen updater. Uses 0x4000 & 0x6000 for data.
/// </summary>
internal class TimexHiResScreenUpdater(
    FrameBuffer frameBuffer,
    IEmulatorMemory memory,
    Color ink,
    Color paper) : IScreenUpdater
{
    private readonly bool[] _dirtyAddresses = new bool[16384];

    public void Update(int frameBufferIndex, Word bitmapAddress, Word attributeAddress, int byteIndex)
    {
        if (byteIndex == 1)
        {
            Update(frameBufferIndex, bitmapAddress);
            Update(frameBufferIndex + 8, (Word)(bitmapAddress + 0x2000));
        }
        else
        {
           Update(frameBufferIndex + 8, bitmapAddress);
           Update(frameBufferIndex + 16, (Word)(bitmapAddress + 0x2000));
        }
    }

    private void Update(int frameBufferIndex, Word bitmapAddress)
    {
        if (!_dirtyAddresses[bitmapAddress])
        {
            return;
        }

        var bitmap = memory.Read((Word)(bitmapAddress + 0x4000));

        for (var bit = 0; bit < FastLookup.BitMasks.Length; bit++)
        {
            var color = (bitmap & FastLookup.BitMasks[bit]) != 0 ? ink : paper;
            frameBuffer.Pixels[frameBufferIndex + bit] = color;
        }

        _dirtyAddresses[bitmapAddress] = false;
    }

    public void Invalidate() => Array.Fill(_dirtyAddresses, true);

    public void SetDirty(int address) => _dirtyAddresses[address - 0x4000] = true;

    public void ToggleFlash()
    {
        // Not supported in this mode
    }
}
using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen.Modes;

/// <summary>
/// Timex HiRes screen updater with doubled effect. Uses second screen for data.
/// </summary>
public class TimexHiResDoubleScreenUpdater(
    FrameBuffer frameBuffer,
    IEmulatorMemory memory,
    Color ink,
    Color paper) : IScreenUpdater
{
    private readonly bool[] _dirtyAddresses = new bool[32 * 24 * 8];

    public void Update(int frameBufferIndex, Word bitmapAddress, Word attributeAddress, int byteIndex)
    {
        if (!_dirtyAddresses[bitmapAddress])
        {
            return;
        }

        var actualBitmapAddress = (Word)(bitmapAddress + 0x6000);

        if (byteIndex == 1)
        {
            Update(frameBufferIndex, actualBitmapAddress);
            Update(frameBufferIndex + 8, actualBitmapAddress);
        }
        else
        {
            Update(frameBufferIndex + 8, actualBitmapAddress);
            Update(frameBufferIndex + 16, actualBitmapAddress);
        }

        _dirtyAddresses[bitmapAddress] = false;
    }

    private void Update(int frameBufferIndex, Word actualBitmapAddress)
    {
        var bitmap = memory.Read(actualBitmapAddress);

        for (var bit = 0; bit < FastLookup.BitMasks.Length; bit++)
        {
            var color = (bitmap & FastLookup.BitMasks[bit]) != 0 ? ink : paper;
            frameBuffer.Pixels[frameBufferIndex + bit] = color;
        }
    }

    public void Invalidate() => Array.Fill(_dirtyAddresses, true);

    public void SetDirty(int address) => _dirtyAddresses[address - 0x6000] = true;

    public void ToggleFlash()
    {
        // Not supported in this mode
    }
}
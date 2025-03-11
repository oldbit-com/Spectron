using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen;

internal sealed class Content(HardwareSettings hardware, FrameBuffer frameBuffer, IEmulatorMemory memory, UlaPlus ulaPlus)
{
    private readonly ScreenRenderEvent[] _screenRenderEvents = FastLookup.GetScreenRenderEvents(hardware);
    private readonly bool[] _bitmapDirty = new bool[32 * 24 * 8];

    private int _frameCount = 1;
    private bool _isFlashOnFrame;
    private int _fetchCycleIndex;

    /// <summary>
    /// Updates the frame buffer with the content of the screen at the specified frame ticks. This allows
    /// proper rendering of the special multicolor effects.
    /// Uses a lookup table to determine the screen byte and attribute address for the current frame tick.
    /// </summary>
    /// <param name="frameTicks">Current ticks at which update is occurring.</param>
    internal void Update(int frameTicks)
    {
        if (frameTicks < hardware.FirstPixelTicks || _fetchCycleIndex >= _screenRenderEvents.Length)
        {
            return;
        }

        while (true)
        {
            var fetchCycleData = _screenRenderEvents[_fetchCycleIndex];

            if (frameTicks < fetchCycleData.Ticks)
            {
                break;
            }

            // First screen byte and attribute
            UpdateFrameBuffer(fetchCycleData.FrameBufferIndex, fetchCycleData.BitmapAddress, fetchCycleData.AttributeAddress);

            // Second screen byte and attribute
            UpdateFrameBuffer(fetchCycleData.FrameBufferIndex + 8, (Word)(fetchCycleData.BitmapAddress + 1),
                (Word)(fetchCycleData.AttributeAddress + 1));

            _fetchCycleIndex += 1;

            if (_fetchCycleIndex >= _screenRenderEvents.Length)
            {
                break;
            }
        }
    }

    internal void NewFrame()
    {
        _frameCount += 1;
        _fetchCycleIndex = 0;

        if (_frameCount < 32)
        {
            return;
        }

        ToggleFlash();
        _frameCount = 1;
    }

    internal void Reset()
    {
        _frameCount += 1;
        _fetchCycleIndex = 0;

        Invalidate();
    }

    internal void Invalidate() => _bitmapDirty.AsSpan().Fill(true);

    internal void UpdateScreen(Word address) => UpdateScreenPrivate(address - 0x4000);

    private void UpdateFrameBuffer(int frameBufferIndex, Word bitmapAddress, Word attributeAddress)
    {
        if (!_bitmapDirty[bitmapAddress])
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

        _bitmapDirty[bitmapAddress] = false;
    }

    private void UpdateScreenPrivate(int address)
    {
        if (address < 0x1800)
        {
            // Single screen byte
            _bitmapDirty[address] = true;
        }
        else
        {
            // Attribute byte affecting 8 screen bytes, unrolled for performance (~7x faster than a for loop)
            var screenAddress = FastLookup.LineAddressForAttrAddress[address - 0x1800];

            _bitmapDirty[screenAddress] = true;
            _bitmapDirty[screenAddress + 256] = true;
            _bitmapDirty[screenAddress + 512] = true;
            _bitmapDirty[screenAddress + 768] = true;
            _bitmapDirty[screenAddress + 1024] = true;
            _bitmapDirty[screenAddress + 1280] = true;
            _bitmapDirty[screenAddress + 1536] = true;
            _bitmapDirty[screenAddress + 1792] = true;
        }
    }

    private void ToggleFlash()
    {
        _isFlashOnFrame = !_isFlashOnFrame;

        for (Word attrAddress = 0x1800; attrAddress < 0x1B00; attrAddress++)
        {
            if ((memory.ReadScreen(attrAddress) & 0x80) != 0)
            {
                UpdateScreenPrivate(attrAddress);
            }
        }
    }
}
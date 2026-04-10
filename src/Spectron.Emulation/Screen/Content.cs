using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Screen.Modes;

namespace OldBit.Spectron.Emulation.Screen;

internal sealed class Content(
    HardwareSettings hardware,
    FrameBuffer frameBuffer,
    IEmulatorMemory memory,
    UlaPlus ulaPlus)
{
    private ScreenRenderEvent[] _screenRenderEvents = FastLookup.GetScreenRenderEvents(hardware, frameBuffer);

    private int _frameCount = 1;
    private int _fetchCycleIndex;

    private IScreenUpdater _screenUpdater = new SpectrumScreenUpdater(frameBuffer, memory, ulaPlus, 0x4000);

    internal void ChangeScreenMode(ScreenMode screenMode, Color ink, Color paper)
    {
        _screenRenderEvents = FastLookup.GetScreenRenderEvents(hardware, frameBuffer, screenMode);

        _screenUpdater = screenMode switch
        {
            ScreenMode.Spectrum => new SpectrumScreenUpdater(frameBuffer, memory, ulaPlus, 0x4000),
            ScreenMode.TimexScreen1 => new SpectrumScreenUpdater(frameBuffer, memory, ulaPlus, 0x6000),
            ScreenMode.TimexHiColor => new TimexHiColorScreenUpdater(frameBuffer, memory, isAlternate: false),
            ScreenMode.TimexHiColorAlt => new TimexHiColorScreenUpdater(frameBuffer, memory, isAlternate: true),
            ScreenMode.TimexHiRes => new TimexHiResScreenUpdater(frameBuffer, memory, ink, paper),
            _ => _screenUpdater
        };
    }

    /// <summary>
    /// Updates the frame buffer with the content of the screen at the specified frame ticks. This allows
    /// proper rendering of the special multicolor effects.
    /// Uses a lookup table to determine the screen byte and attribute address for the current frame tick.
    /// </summary>
    /// <param name="frameTicks">Current ticks at which update is occurring.</param>
    internal void UpdateFrameBuffer(int frameTicks)
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

            // First byte and attribute
            _screenUpdater.Update(fetchCycleData.FrameBufferIndex, fetchCycleData.BitmapAddress,
                fetchCycleData.AttributeAddress, 1);

            // Second byte and attribute
            _screenUpdater.Update(fetchCycleData.FrameBufferIndex + 8, (Word)(fetchCycleData.BitmapAddress + 1),
                (Word)(fetchCycleData.AttributeAddress + 1), 2);

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

        _screenUpdater.ToggleFlash();
        _frameCount = 1;
    }

    internal void Reset()
    {
        _frameCount += 1;
        _fetchCycleIndex = 0;

        Invalidate();
    }

    internal void Invalidate() => _screenUpdater.Invalidate();

    internal void SetDirty(int address) => _screenUpdater.SetDirty(address);
}
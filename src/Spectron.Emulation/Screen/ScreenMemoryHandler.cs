using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen;

internal sealed class ScreenMemoryHandler
{
    private readonly IEmulatorMemory _memory;
    private readonly ScreenBuffer _screenBuffer;

    internal ScreenMode ScreenMode
    {
        set
        {
            if (field == value)
            {
                return;
            }

            field = value; ;
            ChangeScreenMode(value);
        }
    } = ScreenMode.Spectrum;

    internal ScreenMemoryHandler(IEmulatorMemory memory, ScreenBuffer screenBuffer)
    {
        _memory = memory;
        _screenBuffer = screenBuffer;

        memory.MemoryUpdated += SpectrumHandler;
    }

    private void ChangeScreenMode(ScreenMode screenMode)
    {
        RemoveHandlers();

        switch (screenMode)
        {
            case ScreenMode.Spectrum:
                _memory.MemoryUpdated += SpectrumHandler;
                break;

            case ScreenMode.TimexScreen1:
                _memory.MemoryUpdated += TimexScreen1Handler;
                break;

            case ScreenMode.TimexHiColor:
            case ScreenMode.TimexHiColorAlt:
                _memory.MemoryUpdated += TimexHiColorHandler;
                break;

            case ScreenMode.TimexHiRes:
                _memory.MemoryUpdated += TimexHiResHandler;
                break;
        }

        _screenBuffer.ScreenMode = screenMode;
        _screenBuffer.Invalidate();
    }

    private void RemoveHandlers()
    {
        _memory.MemoryUpdated -= SpectrumHandler;
        _memory.MemoryUpdated -= TimexScreen1Handler;
    }

    private void SpectrumHandler(Word address, byte value)
    {
        if (address < 0x5B00)
        {
            _screenBuffer.UpdateScreen(address);
        }
    }

    private void TimexScreen1Handler(Word address, byte value)
    {
        if (address is > 0x5FFF and < 0x7B00)
        {
            _screenBuffer.UpdateScreen(address);
        }
    }

    private void TimexHiColorHandler(Word address, byte value)
    {
        switch (address)
        {
            // Screen data, without attribute data
            case > 0x3FFF and < 0x5800:
            // Attribute data - Timex Screen 1
            case > 0x5FFF and < 0x7800:
                _screenBuffer.UpdateScreen(address);
                break;
        }
    }

    private void TimexHiResHandler(Word address, byte value)
    {
        switch (address)
        {
            // Screen 0 data
            case > 0x3FFF and < 0x5800:
            // Screen 1 data
            case > 0x5FFF and < 0x7800:
                _screenBuffer.UpdateScreen(address);
                break;
        }
    }
}
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen;

internal sealed class ScreenMemoryHandler
{
    private readonly IEmulatorMemory _memory;
    private readonly ScreenBuffer _screenBuffer;
    private bool _isScreenPaged;

    internal ScreenMemoryHandler(IEmulatorMemory memory, ScreenBuffer screenBuffer)
    {
        _memory = memory;
        _screenBuffer = screenBuffer;

        memory.MemoryUpdated += SpectrumHandler;
    }

    internal void SetScreenMode(UlaTimex? ulaTimex, int frameTicks = 0)
    {
        RemoveHandlers();

        _isScreenPaged = false;
        var screenMode = ulaTimex?.ScreenMode ?? ScreenMode.Spectrum;

        switch (screenMode)
        {
            case ScreenMode.Spectrum:
                if (_memory is Memory128K memory128K)
                {
                    memory128K.MemoryUpdated += Spectrum128Handler;
                    memory128K.ScreenBankPaged += Spectrum128ScreenBankPaged;

                    _isScreenPaged = memory128K.IsScreenPaged;
                }
                else
                {
                    _memory.MemoryUpdated += SpectrumHandler;
                }
                break;

            case ScreenMode.TimexSecondScreen:
                _memory.MemoryUpdated += TimexSecondScreenHandler;
                break;

            case ScreenMode.TimexHiColor:
            case ScreenMode.TimexHiColorAlt:
                _memory.MemoryUpdated += TimexHiColorHandler;
                break;

            case ScreenMode.TimexHiRes:
                _memory.MemoryUpdated += TimexHiResHandler;
                break;

            case ScreenMode.TimexHiResAttr:
                _memory.MemoryUpdated += TimexHiResAttrHandler;
                break;

            case ScreenMode.TimexHiResAttrAlt:
                _memory.MemoryUpdated += TimexHiResAttrAltHandler;
                break;

            case ScreenMode.TimexHiResDouble:
                _memory.MemoryUpdated += TimexHiResDoubleHandler;
                break;
        }

        _screenBuffer.ChangeScreenMode(
            screenMode,
            ulaTimex?.Ink ?? SpectrumPalette.Black,
            ulaTimex?.Paper ?? SpectrumPalette.White,
            frameTicks);
    }

    private void Spectrum128ScreenBankPaged(int bankId) =>
        _isScreenPaged = bankId is Memory128K.ScreenBank1 or Memory128K.ScreenBank2;

    private void RemoveHandlers()
    {
        _memory.MemoryUpdated -= SpectrumHandler;
        _memory.MemoryUpdated -= Spectrum128Handler;
        _memory.MemoryUpdated -= TimexSecondScreenHandler;
        _memory.MemoryUpdated -= TimexHiColorHandler;
        _memory.MemoryUpdated -= TimexHiResHandler;
        _memory.MemoryUpdated -= TimexHiResAttrHandler;
        _memory.MemoryUpdated -= TimexHiResAttrAltHandler;
        _memory.MemoryUpdated -= TimexHiResDoubleHandler;

        if (_memory is Memory128K memory128K)
        {
            memory128K.ScreenBankPaged -= Spectrum128ScreenBankPaged;
        }
    }

    private void SpectrumHandler(Word address, byte value)
    {
        if (address is > 0x3FFF and < 0x5B00)
        {
            _screenBuffer.MakeDirty(address);
        }
    }

    private void Spectrum128Handler(Word address, byte value)
    {
        if (_isScreenPaged && address >= 0xC000)
        {
            address -= 0x8000;
        }

        if (address is > 0x3FFF and < 0x5B00)
        {
            _screenBuffer.MakeDirty(address);
        }
    }

    private void TimexSecondScreenHandler(Word address, byte value)
    {
        if (address is > 0x5FFF and < 0x7B00)
        {
            _screenBuffer.MakeDirty(address);
        }
    }

    private void TimexHiColorHandler(Word address, byte value)
    {
        switch (address)
        {
            // Screen data, without attribute data
            case > 0x3FFF and < 0x5800:
            // Attribute data - Timex second screen
            case > 0x5FFF and < 0x7800:
                _screenBuffer.MakeDirty(address);
                break;
        }
    }

    private void TimexHiResHandler(Word address, byte value)
    {
        switch (address)
        {
            // Standard scree data
            case > 0x3FFF and < 0x5800:
            // Second screen data
            case > 0x5FFF and < 0x7800:
                _screenBuffer.MakeDirty(address);
                break;
        }
    }

    private void TimexHiResAttrHandler(Word address, byte value)
    {
        switch (address)
        {
            // Standard screen data and attributes
            case > 0x3FFF and < 0x5B00:
                _screenBuffer.MakeDirty(address);
                break;
        }
    }

    private void TimexHiResAttrAltHandler(Word address, byte value)
    {
        switch (address)
        {
            // Second screen data and attributes
            case > 0x5FFF and < 0x7B00:
                _screenBuffer.MakeDirty(address);
                break;
        }
    }

    private void TimexHiResDoubleHandler(Word address, byte value)
    {
        switch (address)
        {
            // Second screen data
            case > 0x5FFF and < 0x7800:
                _screenBuffer.MakeDirty(address);
                break;
        }
    }
}
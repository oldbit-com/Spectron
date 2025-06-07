using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen;

internal sealed class ScreenBuffer
{
    private readonly Border _border;
    private readonly Content _content;

    private bool _borderColorChanged = true;

    public FrameBuffer FrameBuffer { get; } = new(SpectrumPalette.White);
    internal Color LastBorderColor { get; private set; } = SpectrumPalette.White;

    internal ScreenBuffer(HardwareSettings hardware, IEmulatorMemory memory, UlaPlus ulaPlus)
    {
        _border = new Border(hardware, FrameBuffer);
        _content = new Content(hardware, FrameBuffer, memory, ulaPlus);

        if (memory is Memory128K memory128K)
        {
            memory128K.ScreenBankPaged += _ => { _content.Invalidate(); };
        }
    }

    internal void NewFrame()
    {
        _border.NewFrame();
        _content.NewFrame();
    }

    internal void EndFrame(int frameTicks) =>
        _border.Update(LastBorderColor, frameTicks);

    internal void UpdateBorder(Color borderColor, int frameTicks = 0)
    {
        if (LastBorderColor != borderColor)
        {
            LastBorderColor = borderColor;
            _borderColorChanged = true;
        }

        if (!_borderColorChanged)
        {
            return;
        }

        _border.Update(borderColor, frameTicks);

        _borderColorChanged = false;
    }

    internal void Reset()
    {
        _border.Reset();
        _content.Reset();
    }

    internal void Invalidate()
    {
        _border.Invalidate();
        _content.Invalidate();
    }

    /// <summary>
    /// Called every time when ticks are added to update the frame buffer.
    /// </summary>
    /// <param name="frameTicks">The number of ticks for the current frame.</param>
    internal void UpdateScreen(int frameTicks) => _content.UpdateFrameBuffer(frameTicks);

    /// <summary>
    /// Called when screen memory is updated, so we can mark the address as dirty meaning
    /// frame buffer needs to be updated, too.
    /// </summary>
    /// <param name="address">The address of the screen memory that value has been updated.</param>
    internal void UpdateScreen(Word address) => _content.MakeDirty(address);
}
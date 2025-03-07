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
    /// Called every time new border color is set to update the frame buffer.
    /// </summary>
    /// <param name="frameTicks">The number of ticks for the current frame.</param>
    internal void UpdateBorder(int frameTicks) => _border.Update(LastBorderColor, frameTicks);

    /// <summary>
    /// Called every time when ticks are added to update the frame buffer..
    /// </summary>
    /// <param name="frameTicks">The number of ticks for the current frame.</param>
    internal void UpdateContent(int frameTicks) => _content.Update(frameTicks);

    /// <summary>
    /// Called when screen memory is updated to mark the specified address as dirty, flagging the frame buffer
    /// needs to be updated.
    /// </summary>
    /// <param name="address">The address of the screen memory that has been written to.</param>
    internal void UpdateScreen(Word address) => _content.UpdateScreen(address);
}
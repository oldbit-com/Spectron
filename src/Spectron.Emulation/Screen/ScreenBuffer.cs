using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;

namespace OldBit.Spectron.Emulation.Screen;

internal sealed class ScreenBuffer
{
    private readonly Border _border;
    private readonly Content _content;

    private bool _borderColorChanged = true;

    public FrameBuffer FrameBuffer { get; } = new(SpectrumPalette.White);
    internal Color LastBorderColor { get; private set; }

    internal ScreenBuffer(HardwareSettings hardware, IEmulatorMemory memory, UlaPlus ulaPlus)
    {
        _border = new Border(FrameBuffer);
        _content = new Content(hardware, FrameBuffer, memory, ulaPlus);
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

    internal void UpdateBorder(int frameTicks) => _border.Update(LastBorderColor, frameTicks);

    internal void UpdateContent(int frameTicks) => _content.Update(frameTicks);

    internal void UpdateScreen(Word address) => _content.UpdateScreen(address);
}
using OldBit.Spectral.Emulation.Devices;
using OldBit.Spectral.Emulation.Devices.Memory;

namespace OldBit.Spectral.Emulation.Screen;

internal class ScreenBuffer
{
    private readonly Border _border;
    private readonly Content _content;

    private Color _lastBorderColor;
    private bool _borderColorChanged = true;

    public FrameBuffer FrameBuffer { get; } = new(Palette.White);

    internal ScreenBuffer(IEmulatorMemory memory, UlaPlus ulaPlus)
    {
        _border = new Border(FrameBuffer);
        _content = new Content(FrameBuffer, memory, ulaPlus);
    }

    internal void NewFrame()
    {
        _border.NewFrame();
        _content.NewFrame();
    }

    internal void UpdateBorder(Color borderColor, int frameTicks = 0)
    {
        if (_lastBorderColor != borderColor)
        {
            _lastBorderColor = borderColor;
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

    internal void UpdateBorder(int frameTicks) => _border.Update(_lastBorderColor, frameTicks);

    internal void UpdateContent(int frameTicks) => _content.Update(frameTicks);

    internal void UpdateScreen(Word address) => _content.UpdateScreen(address);


}
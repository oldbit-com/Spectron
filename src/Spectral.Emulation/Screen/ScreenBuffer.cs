using OldBit.Spectral.Emulation.Devices.Memory;

namespace OldBit.Spectral.Emulation.Screen;

internal class ScreenBuffer
{
    private readonly Border _border;
    private readonly Content _content;

    private Color _lastBorderColor;
    private bool _borderColorChanged = true;

    public FrameBuffer FrameBuffer { get; } = new(Colors.White);

    public ScreenBuffer(IEmulatorMemory memory)
    {
        _border = new Border(FrameBuffer);
        _content = new Content(FrameBuffer, memory);
    }

    public void NewFrame()
    {
        _border.NewFrame();
        _content.NewFrame();
    }

    public void UpdateBorder(Color borderColor, int frameTicks = 0)
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

    public void Reset()
    {
        _border.Reset();
        _content.Reset();
    }

    public void UpdateBorder(int frameTicks) => _border.Update(_lastBorderColor, frameTicks);

    public void UpdateContent(int frameTicks) => _content.Update(frameTicks);

    public void UpdateScreen(Word address) => _content.UpdateScreen(address);
}
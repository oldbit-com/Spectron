using OldBit.Spectral.Emulator.Hardware;

namespace OldBit.Spectral.Emulator.Screen;

internal class ScreenRenderer
{
    private readonly BorderRenderer _borderRenderer;
    private readonly ContentRenderer _contentRenderer;

    private Color _lastBorderColor;
    private bool _borderColorChanged = true;

    public FrameBuffer FrameBuffer { get; } = new(Colors.White);

    public ScreenRenderer(Memory48K memory)
    {
        _borderRenderer = new BorderRenderer(FrameBuffer);
        _contentRenderer = new ContentRenderer(FrameBuffer, memory);
    }

    public void NewFrame()
    {
        _borderRenderer.NewFrame();
        _contentRenderer.NewFrame();
    }

    public void UpdateBorder(Color borderColor, int frameTicks)
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

        _borderRenderer.Update(borderColor, frameTicks);

        _borderColorChanged = false;
    }

    public void Reset()
    {
        _borderRenderer.Reset();
        _contentRenderer.Reset();
    }

    public void UpdateBorder(int frameTicks) => _borderRenderer.Update(_lastBorderColor, frameTicks);

    public void UpdateContent(int frameTicks) => _contentRenderer.Update(frameTicks);

    public void ScreenMemoryUpdated(Word address) => _contentRenderer.UpdateScreen(address);
}
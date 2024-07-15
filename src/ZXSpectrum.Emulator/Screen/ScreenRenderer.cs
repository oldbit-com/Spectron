using OldBit.ZXSpectrum.Emulator.Hardware;

namespace OldBit.ZXSpectrum.Emulator.Screen;

public class ScreenRenderer
{
    public const int TotalWidth = Constants.BorderLeft + Constants.ContentWidth + Constants.BorderRight;
    public const int TotalHeight = Constants.BorderTop + Constants.ContentHeight + Constants.BorderBottom;

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

    public void UpdateBorder(Color borderColor, int currentTicks)
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

        _borderRenderer.Update(borderColor, currentTicks);
        _borderColorChanged = false;
    }

    public void UpdateBorder(int currentTicks) => _borderRenderer.Update(_lastBorderColor, currentTicks);

    public void UpdateContent(int currentFrameTicks) => _contentRenderer.Update(currentFrameTicks);

    public void ScreenMemoryUpdated(Word address) => _contentRenderer.UpdateScreen(address);
}
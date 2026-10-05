using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Extensions;

namespace OldBit.Spectron.Emulation.Screen;

public sealed class ScreenBuffer
{
    private readonly Border _border;
    private readonly Content _content;
    private readonly UlaPlus _ulaPlus;

    private bool _borderColorChanged = true;
    private Color? _lockedBorderColor;
    private byte _lockedBorderColorIndex;
    private ScreenMode _screenMode = ScreenMode.Spectrum;

    public FrameBuffer FrameBuffer { get; }

    internal byte LastBorderColorIndex { get; private set; } = 0x07;

    public event EventHandler<EventArgs>? FrameBufferChanged;

    internal ScreenBuffer(HardwareSettings hardware, IEmulatorMemory memory, UlaPlus ulaPlus)
    {
        FrameBuffer = new FrameBuffer();

        _ulaPlus = ulaPlus;
        _border = new Border(hardware, FrameBuffer);
        _content = new Content(hardware, FrameBuffer, memory, ulaPlus);

        if (memory is Memory128K memory128K)
        {
            memory128K.ScreenBankPaged += _ => { _content.Invalidate(); };
        }
    }

    internal void ChangeScreenMode(ScreenMode screenMode, Color ink, Color paper, byte paperIndex, int frameTicks)
    {
        _lockedBorderColor = screenMode.IsTimexHiRes() ? paper : null;
        _lockedBorderColorIndex = paperIndex;

        _content.ChangeScreenMode(screenMode, ink, paper);
        _border.ChangeScreenMode(screenMode);

        _border.Update(BorderColor, frameTicks);

        if (_screenMode != screenMode)
        {
            FrameBufferChanged?.Invoke(this, EventArgs.Empty);
        }

        _screenMode = screenMode;
    }

    internal void NewFrame()
    {
        _border.NewFrame();
        _content.NewFrame();
    }

    internal void EndFrame(int frameTicks) => _border.Update(BorderColor, frameTicks);

    internal void UpdateBorder(byte borderColor, int frameTicks = 0)
    {
        borderColor &= 0x07;

        if (LastBorderColorIndex != borderColor)
        {
            LastBorderColorIndex = borderColor;
            _borderColorChanged = true;
        }

        if (!_borderColorChanged)
        {
            return;
        }

        _border.Update(BorderColor, frameTicks);

        _borderColorChanged = false;
    }

    internal void RefreshBorder(int frameTicks = 0)
    {
        _borderColorChanged = true;

        UpdateBorder(LastBorderColorIndex, frameTicks);
    }

    private Color BorderColor
    {
        get
        {
            if (_ulaPlus is { IsEnabled: true, IsActive: true })
            {
                return _lockedBorderColor != null ?
                    _ulaPlus.GetBorderColor(_lockedBorderColorIndex, isHiRes: true) :
                    _ulaPlus.GetBorderColor(LastBorderColorIndex);
            }

            return _lockedBorderColor ?? SpectrumPalette.GetBorderColor(LastBorderColorIndex);
        }
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
    internal void MakeDirty(Word address) => _content.MakeDirty(address);
}
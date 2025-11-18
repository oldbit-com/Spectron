namespace OldBit.Spectron.Emulation.Devices.Mouse;

public sealed class MouseManager
{
    private readonly SpectrumBus _spectrumBus;
    private IMouse? Mouse { get; set; }

    public MouseType MouseType { get; private set; }

    internal MouseManager(SpectrumBus spectrumBus) => _spectrumBus = spectrumBus;

    public void UpdatePosition(int x, int y)
    {
        if (Mouse == null)
        {
            return;
        }

        Mouse.X = (byte)x;
        Mouse.Y = (byte)y;
    }

    public void UpdateMouseButtons(MouseButtons pressedButtons)
    {
        if (Mouse == null)
        {
            return;
        }

        Mouse.Buttons = pressedButtons;
    }

    public void Configure(MouseType mouseType)
    {
        MouseType = mouseType;
        _spectrumBus.RemoveDevice(Mouse);

        Mouse = mouseType switch
        {
            MouseType.Kempston => new KempstonMouse(),
            _ => (IMouse?)null
        };

        if (Mouse != null)
        {
            _spectrumBus.AddDevice(Mouse);
        }
    }
}
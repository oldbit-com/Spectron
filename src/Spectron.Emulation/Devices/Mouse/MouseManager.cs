namespace OldBit.Spectron.Emulation.Devices.Mouse;

public sealed class MouseManager
{
    public KempstonMouse Mouse { get; } = new();

    public bool IsEnabled => Mouse.IsEnabled;

    public void Enable()
    {
        Mouse.IsEnabled = true;
    }

    public void Disable()
    {
        Mouse.IsEnabled = false;
    }

    public void UpdatePosition(int x, int y)
    {
        Mouse.X = (byte)x;
        Mouse.Y = (byte)y;
    }

    public void UpdateMouseButtons(MouseButtons pressedButtons) =>
        Mouse.Buttons = pressedButtons;
}
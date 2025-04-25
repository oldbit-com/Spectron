namespace OldBit.Spectron.Emulation.Devices.Mouse;

public sealed class MouseManager
{
    public KempstonMouse Mouse { get; } = new();

    public void UpdatePosition(int x, int y)
    {
        Mouse.X = (byte)x;
        Mouse.Y = (byte)y;
    }

    public void UpdateMouseButtons(MouseButtons pressedButtons) =>
        Mouse.Buttons = pressedButtons;
}
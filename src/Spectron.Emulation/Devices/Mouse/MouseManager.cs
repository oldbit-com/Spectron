namespace OldBit.Spectron.Emulation.Devices.Mouse;

public sealed class MouseManager
{
    public KempstonMouse Mouse { get; } = new KempstonMouse();

    public void Enable()
    {
        Mouse.IsEnabled = true;
    }

    public void Disable()
    {
        Mouse.IsEnabled = false;
    }
}
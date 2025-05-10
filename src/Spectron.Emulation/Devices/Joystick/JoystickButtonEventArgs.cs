namespace OldBit.Spectron.Emulation.Devices.Joystick;

public class JoystickButtonEventArgs(JoystickInput input) : EventArgs
{
    public JoystickInput Input { get; } = input;
}
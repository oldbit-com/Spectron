namespace OldBit.Spectron.Emulation.Devices.Joystick;

public class JoystickButtonEventArgs(JoystickInput input, InputState state) : EventArgs
{
    public JoystickInput Input { get; } = input;

    public InputState State { get; } = state;
}
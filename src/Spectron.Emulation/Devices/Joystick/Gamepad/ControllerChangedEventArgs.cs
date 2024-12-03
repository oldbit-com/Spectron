namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public class ControllerChangedEventArgs(GamepadController controller, ControllerChangedAction action) : EventArgs
{
    public ControllerChangedAction Action { get; } = action;

    public GamepadController Controller { get; } = controller;
}
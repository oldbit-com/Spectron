namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public class ValueChangedEventArgs(int controlId, int? value) : EventArgs
{
    public int ControlId { get; } = controlId;

    public int? Value { get; set; } = value;
}
namespace OldBit.Spectral.Emulation.Devices.Joystick;

internal class FullerJoystick : IJoystick
{
    private const Word FullerPort = 0x7F;
    private const byte None = 0xFF;
    private const byte Right = 0x08;
    private const byte Left = 0x04;
    private const byte Down = 0x02;
    private const byte Up = 0x01;
    private const byte Fire = 0x80;

    private byte _joystickState = None;

    public byte? ReadPort(Word address) => address != FullerPort ? null : _joystickState;

    public void HandleInput(JoystickInput input, bool isOn)
    {
        var value = GetValue(input);

        if (isOn)
        {
            _joystickState &= (byte)~value;
        }
        else
        {
            _joystickState |= value;
        }
    }

    private static byte GetValue(JoystickInput input) => input switch
    {
        JoystickInput.Left => Left,
        JoystickInput.Right => Right,
        JoystickInput.Up => Up,
        JoystickInput.Down => Down,
        JoystickInput.Fire => Fire,
        _ => None
    };
}
namespace OldBit.Spectron.Emulation.Devices.Joystick.Providers;

internal class KempstonJoystick : IJoystick
{
    private const Word KempstonPort = 0x1F;
    private const byte None = 0x0;
    private const byte Right = 0x01;
    private const byte Left = 0x02;
    private const byte Down = 0x04;
    private const byte Up = 0x08;
    private const byte Fire = 0x10;

    private byte _joystickState = None;

    public byte? ReadPort(Word address) => (address & 0x1F) != KempstonPort ? null : _joystickState;

    public void HandleInput(JoystickInput input, bool isOn)
    {
        var value = GetValue(input);

        if (isOn)
        {
            _joystickState |= value;
        }
        else
        {
            _joystickState &= (byte)~value;
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
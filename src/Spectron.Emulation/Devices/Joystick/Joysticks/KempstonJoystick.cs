namespace OldBit.Spectron.Emulation.Devices.Joystick.Joysticks;

internal class KempstonJoystick : IJoystick
{
    private const Word KempstonPortMask = 0xE0; // Partially decoded: ---- ---- 000- ----
    private const byte None = 0x0;
    private const byte Right = 0x01;
    private const byte Left = 0x02;
    private const byte Down = 0x04;
    private const byte Up = 0x08;
    private const byte Fire = 0x10;

    private byte _joystickState = None;

    public byte? ReadPort(Word address) => (address & KempstonPortMask) == 0 ? _joystickState : null;

    public void HandleInput(JoystickInput input, InputState state)
    {
        var value = GetValue(input);

        if (state == InputState.Pressed)
        {
            _joystickState |= value;
        }
        else
        {
            _joystickState &= (byte)~value;
        }
    }

    public int Priority => 1000;

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
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Joysticks;

public class CursorJoystick(KeyboardState keyboardState) : IJoystick
{
    private readonly Dictionary<JoystickInput, List<SpectrumKey>> _joyToKeyMapping = new()
    {
        { JoystickInput.Left, [SpectrumKey.CapsShift, SpectrumKey.D5] },
        { JoystickInput.Right, [SpectrumKey.CapsShift, SpectrumKey.D8] },
        { JoystickInput.Up, [SpectrumKey.CapsShift, SpectrumKey.D7] },
        { JoystickInput.Down, [SpectrumKey.CapsShift, SpectrumKey.D6] },
        { JoystickInput.Fire, [SpectrumKey.CapsShift, SpectrumKey.D0] }
    };

    public void HandleInput(JoystickInput input, InputState state)
    {
        if (state == InputState.Pressed)
        {
            if (_joyToKeyMapping.TryGetValue(input, out var keys))
            {
                keyboardState.KeyDown(keys);
            }
        }
        else
        {
            if (_joyToKeyMapping.TryGetValue(input, out var keys))
            {
                keyboardState.KeyUp(keys);
            }
        }
    }
}
using OldBit.Spectral.Emulation.Devices.Keyboard;

namespace OldBit.Spectral.Emulation.Devices.Joystick;

public class CursorJoystick(KeyboardHandler keyboardHandler) : IJoystick
{
    private readonly Dictionary<JoystickInput, List<SpectrumKey>> _joyToKeyMapping = new()
    {
        { JoystickInput.Left, [SpectrumKey.CapsShift, SpectrumKey.D5] },
        { JoystickInput.Right, [SpectrumKey.CapsShift, SpectrumKey.D8] },
        { JoystickInput.Up, [SpectrumKey.CapsShift, SpectrumKey.D7] },
        { JoystickInput.Down, [SpectrumKey.CapsShift, SpectrumKey.D6] },
        { JoystickInput.Fire, [SpectrumKey.CapsShift, SpectrumKey.D0] }
    };

    public void HandleInput(JoystickInput input, bool isOn)
    {
        if (isOn)
        {
            if (_joyToKeyMapping.TryGetValue(input, out var keys))
            {
                keyboardHandler.HandleKeyDown(keys);
            }
        }
        else
        {
            if (_joyToKeyMapping.TryGetValue(input, out var keys))
            {
                keyboardHandler.HandleKeyUp(keys);
            }
        }
    }
}
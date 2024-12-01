using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Joysticks;

public class SinclairJoystick(KeyboardHandler keyboardHandler, JoystickType joystickType) : IJoystick
{
    private readonly Dictionary<JoystickType, Dictionary<JoystickInput, SpectrumKey>> _joyToKeyMapping = new()
    {
        {
            JoystickType.Sinclair1, new Dictionary<JoystickInput, SpectrumKey>()
            {
                { JoystickInput.Left, SpectrumKey.D1 },
                { JoystickInput.Right, SpectrumKey.D2 },
                { JoystickInput.Up, SpectrumKey.D4 },
                { JoystickInput.Down, SpectrumKey.D3 },
                { JoystickInput.Fire, SpectrumKey.D5 }
            }
        },
        {
            JoystickType.Sinclair2, new Dictionary<JoystickInput, SpectrumKey>()
            {
                { JoystickInput.Left, SpectrumKey.D6 },
                { JoystickInput.Right, SpectrumKey.D7 },
                { JoystickInput.Up, SpectrumKey.D9 },
                { JoystickInput.Down, SpectrumKey.D8 },
                { JoystickInput.Fire, SpectrumKey.D0 }
            }
        }
    };

    public void HandleInput(JoystickInput input, InputState state)
    {
        if (state == InputState.Pressed)
        {
            if (_joyToKeyMapping[joystickType].TryGetValue(input, out var key))
            {
                keyboardHandler.HandleKeyDown([key]);
            }
        }
        else
        {
            if (_joyToKeyMapping[joystickType].TryGetValue(input, out var key))
            {
                keyboardHandler.HandleKeyUp([key]);
            }
        }
    }
}
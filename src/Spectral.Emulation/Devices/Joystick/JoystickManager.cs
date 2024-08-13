using OldBit.Spectral.Emulation.Devices.Keyboard;

namespace OldBit.Spectral.Emulation.Devices.Joystick;

public sealed class JoystickManager
{
    private readonly SpectrumBus _spectrumBus;
    private readonly KeyboardHandler _keyboardHandler;
    private IJoystick? _joystick;

    internal JoystickManager(SpectrumBus spectrumBus, KeyboardHandler keyboardHandler)
    {
        _spectrumBus = spectrumBus;
        _keyboardHandler = keyboardHandler;
    }

    public void SetupJoystick(JoystickType joystickType)
    {
        _spectrumBus.RemoveDevice(_joystick);

        _joystick = joystickType switch
        {
            JoystickType.Kempston => new KempstonJoystick(),
            JoystickType.Fuller => new FullerJoystick(),
            JoystickType.Cursor => new CursorJoystick(_keyboardHandler),
            JoystickType.Sinclair1 => new SinclairJoystick(_keyboardHandler, JoystickType.Sinclair1),
            JoystickType.Sinclair2 => new SinclairJoystick(_keyboardHandler, JoystickType.Sinclair2),
            _ => null
        };

        if (_joystick != null)
        {
            _spectrumBus.AddDevice(_joystick);
        }
    }

    public void HandleInput(JoystickInput input, bool isOn) =>
        _joystick?.HandleInput(input, isOn);
}
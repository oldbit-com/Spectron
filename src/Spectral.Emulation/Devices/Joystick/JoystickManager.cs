namespace OldBit.Spectral.Emulation.Devices.Joystick;

public sealed class JoystickManager
{
    private readonly SpectrumBus _spectrumBus;
    private IJoystick? _joystick;

    internal JoystickManager(SpectrumBus spectrumBus)
    {
        _spectrumBus = spectrumBus;
    }

    public void SetupJoystick(JoystickType joystickType)
    {
        if (_joystick != null)
        {
            _spectrumBus.RemoveDevice(_joystick);
            _joystick = null;
        }

        _joystick = joystickType switch
        {
            JoystickType.Kempston => new KempstonJoystick(),
            JoystickType.Fuller => new FullerJoystick(),
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
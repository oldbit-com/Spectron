using System.Collections.ObjectModel;
using OldBit.Spectron.Emulation.Devices.Joystick.GamePad;
using OldBit.Spectron.Emulation.Devices.Joystick.Providers;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Emulation.Devices.Joystick;

public sealed class JoystickManager
{
    private readonly GamePadManager _gamePadManager;
    private readonly SpectrumBus _spectrumBus;
    private readonly KeyboardHandler _keyboardHandler;
    private IJoystick? _joystick;

    public JoystickType JoystickType { get; private set; } = JoystickType.None;

    internal JoystickManager(GamePadManager gamePadManager, SpectrumBus spectrumBus, KeyboardHandler keyboardHandler)
    {
        _gamePadManager = gamePadManager;
        _spectrumBus = spectrumBus;
        _keyboardHandler = keyboardHandler;
    }

    public void SetupJoystick(JoystickType joystickType)
    {
        JoystickType = joystickType;

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

    public void HandleInput(JoystickInput input, bool isOn) => _joystick?.HandleInput(input, isOn);
}
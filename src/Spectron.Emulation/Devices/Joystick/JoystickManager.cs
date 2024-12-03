using System.Diagnostics;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick.Joysticks;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Emulation.Devices.Joystick;

public sealed class JoystickManager
{
    private readonly Stopwatch _stopwatch = new();
    private readonly GamepadManager _gamepadManager;
    private readonly SpectrumBus _spectrumBus;
    private readonly KeyboardHandler _keyboardHandler;
    private IJoystick? _joystick;

    public JoystickType JoystickType { get; private set; } = JoystickType.None;

    internal JoystickManager(
        GamepadManager gamepadManager,
        SpectrumBus spectrumBus,
        KeyboardHandler keyboardHandler)
    {
        _gamepadManager = gamepadManager;
        _spectrumBus = spectrumBus;
        _keyboardHandler = keyboardHandler;

        _stopwatch.Start();
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

        _gamepadManager.Initialize();
    }

    public void Update()
    {
        if (_joystick == null)
        {
            return;
        }

        // If running too fast, skip the update, normally 50Hz / 20ms - may move this to UI code
        if (_stopwatch.ElapsedMilliseconds < 19)
        {
            return;
        }

        _gamepadManager.Update();
        UpdateAllInputsState();

        _stopwatch.Restart();
    }

    public void Pressed(JoystickInput input) => _joystick?.HandleInput(input, InputState.Pressed);

    public void Released(JoystickInput input) => _joystick?.HandleInput(input, InputState.Released);

    private void UpdateAllInputsState()
    {
        UpdateInputState(JoystickInput.Up);
        UpdateInputState(JoystickInput.Right);
        UpdateInputState(JoystickInput.Down);
        UpdateInputState(JoystickInput.Left);
        UpdateInputState(JoystickInput.Fire);
    }

    private void UpdateInputState(JoystickInput input)
    {
        var inputState = _gamepadManager.GetInputState(input);

        _joystick?.HandleInput(input, inputState);
    }
}
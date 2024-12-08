using System.Timers;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick.Joysticks;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using Timer = System.Timers.Timer;

namespace OldBit.Spectron.Emulation.Devices.Joystick;

public sealed class JoystickManager
{
    private readonly TimeSpan _joystickUpdateInterval = TimeSpan.FromMilliseconds(20);

    private readonly GamepadManager _gamepadManager;
    private readonly SpectrumBus _spectrumBus;
    private readonly KeyboardState _keyboardState;
    private readonly Timer _updateTimer;

    private IJoystick? _joystick;

    public JoystickType JoystickType { get; private set; } = JoystickType.None;

    internal JoystickManager(
        GamepadManager gamepadManager,
        SpectrumBus spectrumBus,
        KeyboardState keyboardState)
    {
        _gamepadManager = gamepadManager;
        _spectrumBus = spectrumBus;
        _keyboardState = keyboardState;

        _updateTimer = new Timer(_joystickUpdateInterval) { AutoReset = true };
        _updateTimer.Elapsed += UpdateJoystickState;
    }

    public void SetupJoystick(JoystickType joystickType)
    {
        _updateTimer.Stop();

        JoystickType = joystickType;

        _spectrumBus.RemoveDevice(_joystick);

        _joystick = joystickType switch
        {
            JoystickType.Kempston => new KempstonJoystick(),
            JoystickType.Fuller => new FullerJoystick(),
            JoystickType.Cursor => new CursorJoystick(_keyboardState),
            JoystickType.Sinclair1 => new SinclairJoystick(_keyboardState, JoystickType.Sinclair1),
            JoystickType.Sinclair2 => new SinclairJoystick(_keyboardState, JoystickType.Sinclair2),
            _ => null
        };

        _gamepadManager.Initialize();

        if (_joystick != null)
        {
            _spectrumBus.AddDevice(_joystick);
            _updateTimer.Start();
        }
    }

    public void Pressed(JoystickInput input) => _joystick?.HandleInput(input, InputState.Pressed);

    public void Released(JoystickInput input) => _joystick?.HandleInput(input, InputState.Released);

    public void Stop()
    {
        _updateTimer.Dispose();
        _spectrumBus.RemoveDevice(_joystick);
    }

    private void UpdateJoystickState(object? sender, ElapsedEventArgs e)
    {
        if (_joystick == null)
        {
            return;
        }

        if (!_gamepadManager.Update())
        {
            return;
        }

        UpdateInputState(JoystickInput.Up);
        UpdateInputState(JoystickInput.Right);
        UpdateInputState(JoystickInput.Down);
        UpdateInputState(JoystickInput.Left);
        UpdateInputState(JoystickInput.Fire);
    }

    private void UpdateInputState(JoystickInput input)
    {
        var inputState = _gamepadManager.GetJoystickInputState(input);

        _joystick?.HandleInput(input, inputState);
    }
}
using System.Timers;
using OldBit.Spectron.Emulation.Devices.Gamepad;
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
    private readonly Timer _gamepadUpdateTimer;

    private IJoystick? _joystick;

    public event EventHandler<JoystickButtonEventArgs>? JoystickButtonChanged;

    public JoystickType JoystickType { get; private set; } = JoystickType.None;

    internal JoystickManager(
        GamepadManager gamepadManager,
        SpectrumBus spectrumBus,
        KeyboardState keyboardState)
    {
        _gamepadManager = gamepadManager;
        _spectrumBus = spectrumBus;
        _keyboardState = keyboardState;

        _gamepadUpdateTimer = new Timer(_joystickUpdateInterval) { AutoReset = true };
        _gamepadUpdateTimer.Elapsed += GamepadUpdateJoystickState;
    }

    public void SetupJoystick(JoystickType joystickType)
    {
        _gamepadUpdateTimer.Stop();

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
            _gamepadUpdateTimer.Start();
        }
    }

    public void Pressed(JoystickInput input) => OnJoystickButton(input, InputState.Pressed);

    public void Released(JoystickInput input) => OnJoystickButton(input, InputState.Released);

    public void Stop()
    {
        _gamepadUpdateTimer.Dispose();
        _spectrumBus.RemoveDevice(_joystick);
    }

    private void OnJoystickButton(JoystickInput input, InputState state)
    {
        _joystick?.HandleInput(input, state);

        JoystickButtonChanged?.Invoke(this, new JoystickButtonEventArgs(input));
    }

    private void GamepadUpdateJoystickState(object? sender, ElapsedEventArgs e)
    {
        if (_joystick == null)
        {
            return;
        }

        if (!_gamepadManager.Update())
        {
            return;
        }

        GamepadUpdateInputState(JoystickInput.Up);
        GamepadUpdateInputState(JoystickInput.Right);
        GamepadUpdateInputState(JoystickInput.Down);
        GamepadUpdateInputState(JoystickInput.Left);
        GamepadUpdateInputState(JoystickInput.Fire);
    }

    private void GamepadUpdateInputState(JoystickInput input)
    {
        var inputState = _gamepadManager.GetJoystickInputState(input);

        OnJoystickButton(input, inputState);
    }
}
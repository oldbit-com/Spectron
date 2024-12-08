using OldBit.Joypad;
using OldBit.Joypad.Controls;
using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public record GamepadPreferences(
    Guid ControllerId,
    JoystickType JoystickType,
    List<GamepadMapping> InputMappings);

public sealed class GamepadManager
{
    private readonly KeyboardState _keyboardState;
    private readonly CommandManager _commandManager;
    private readonly JoypadManager _joypadManager;
    private readonly List<GamepadController> _controllers = [];

    private GamepadPreferences? _activeGamepad;
    private Dictionary<JoystickInput, List<GamepadMapping>> _joystickInputMappings = new();
    private Dictionary<int, GamepadAction> _keyboardMappings = new();
    private Dictionary<int, GamepadAction> _commandMappings = new();

    private bool _initialized;

    public IReadOnlyList<GamepadController> Controllers => _controllers;

    public event EventHandler<ControllerChangedEventArgs>? ControllerChanged;

    public GamepadManager(KeyboardState keyboardState, CommandManager commandManager)
    {
        _keyboardState = keyboardState;
        _commandManager = commandManager;
        _joypadManager = new JoypadManager();

        _joypadManager.ControllerConnected += HandleControllerConnected;
        _joypadManager.ControllerDisconnected += HandleControllerDisconnected;

        _controllers.Add(GamepadController.None);
        HandleControllerChanged(GamepadController.None, ControllerChangedAction.Added);
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _joypadManager.Start();
    }

    public void Stop()
    {
        _joypadManager.Stop();
        _joypadManager.Dispose();
    }

    public void Setup(GamepadPreferences gamepad)
    {
        if (gamepad.ControllerId == Guid.Empty || gamepad.JoystickType == JoystickType.None)
        {
            _activeGamepad = null;
            _joystickInputMappings = [];

            return;
        }

        _activeGamepad = gamepad;

        _joystickInputMappings = gamepad.InputMappings
            .Where(mapping =>
                mapping.Action is
                    GamepadAction.JoystickLeft or
                    GamepadAction.JoystickRight or
                    GamepadAction.JoystickUp or
                    GamepadAction.JoystickDown or
                    GamepadAction.JoystickFire)
            .GroupBy(mapping => mapping.Action)
            .ToDictionary(
                group => group.Key switch
                {
                    GamepadAction.JoystickLeft => JoystickInput.Left,
                    GamepadAction.JoystickRight => JoystickInput.Right,
                    GamepadAction.JoystickUp => JoystickInput.Up,
                    GamepadAction.JoystickDown => JoystickInput.Down,
                    GamepadAction.JoystickFire => JoystickInput.Fire,
                    _ => JoystickInput.None
                },
                group => group.ToList());

        _keyboardMappings = gamepad.InputMappings
            .Where(mapping => mapping.Action is >= GamepadAction.D0 and <= GamepadAction.Z)
            .ToDictionary(mapping => mapping.ControlId, mapping => mapping.Action);

        _commandMappings = gamepad.InputMappings
            .Where(mapping => mapping.Action is >= GamepadAction.Pause and <= GamepadAction.TimeTravel)
            .ToDictionary(mapping => mapping.ControlId, mapping => mapping.Action);
    }

    public bool Update()
    {
        if (_activeGamepad == null)
        {
            return false;
        }

        _joypadManager.Update(_activeGamepad.ControllerId);

        return true;
    }

    public void Update(Guid controllerId) => _joypadManager.Update(controllerId);

    public InputState GetJoystickInputState(JoystickInput input)
    {
        if (_activeGamepad == null || !_joystickInputMappings.TryGetValue(input, out var inputMappings))
        {
            return InputState.Released;
        }

        if (!_joypadManager.TryGetController(_activeGamepad.ControllerId, out var controller))
        {
            return InputState.Released;
        }

        foreach (var inputMapping in inputMappings)
        {
            if (!controller.TryGetControl(inputMapping.ControlId, out var control))
            {
                return InputState.Released;
            }

            if (control.Value == null)
            {
                continue;
            }

            if (control.ControlType == ControlType.DirectionalPad)
            {
                if ((inputMapping.Direction & (DirectionalPadDirection)control.Value) != DirectionalPadDirection.None)
                {
                    return InputState.Pressed;
                }
            }
            else if (control.IsPressed)
            {
                return InputState.Pressed;
            }
        }

        return InputState.Released;
    }

    private void HandleControllerChanged(GamepadController controller, ControllerChangedAction action) =>
        ControllerChanged?.Invoke(this, new ControllerChangedEventArgs(controller, action));

    private void HandleControllerConnected(object? sender, JoypadControllerEventArgs e)
    {
        if (_controllers.Any(controller => controller.ControllerId == e.Controller.Id))
        {
            return;
        }

        var buttons = e.Controller.Controls
            .Where(control => control.ControlType is ControlType.Button)
            .Select(button => new GamepadButton(button.Id, button.Name))
            .ToList();

        var dpad = e.Controller.Controls
            .FirstOrDefault(control => control.ControlType is ControlType.DirectionalPad);

        if (dpad != null)
        {
            buttons.Insert(0, new GamepadButton(dpad.Id, "D-Pad Left", DirectionalPadDirection.Left));
            buttons.Insert(1, new GamepadButton(dpad.Id, "D-Pad Up", DirectionalPadDirection.Up));
            buttons.Insert(2, new GamepadButton(dpad.Id, "D-Pad Right", DirectionalPadDirection.Right));
            buttons.Insert(3, new GamepadButton(dpad.Id, "D-Pad Down", DirectionalPadDirection.Down));
        }

        var controller = new GamepadController(e.Controller, buttons);
        controller.ValueChanged += HandleControllerValueChanged;

        _controllers.Add(controller);
        HandleControllerChanged(controller, ControllerChangedAction.Added);
    }

    private void HandleControllerDisconnected(object? sender, JoypadControllerEventArgs e)
    {
        var existingController = _controllers.FirstOrDefault(x => x.ControllerId == e.Controller.Id);

        if (existingController == null)
        {
            return;
        }

        existingController.ValueChanged -= HandleControllerValueChanged;
        _controllers.Remove(existingController);

        HandleControllerChanged(existingController, ControllerChangedAction.Removed);
    }

    private void HandleControllerValueChanged(object? sender, ValueChangedEventArgs e)
    {
        HandleKeyboardAction(e);
        HandleCommandAction(e);
    }

    private void HandleKeyboardAction(ValueChangedEventArgs e)
    {
        if (!_keyboardMappings.TryGetValue(e.ControlId, out var action))
        {
            return;
        }

        var key = action.ToSpectrumKey();

        if (key == null)
        {
            return;
        }

        if (e.IsPressed)
        {
            _keyboardState.KeyDown([key.Value]);
        }
        else
        {
            _keyboardState.KeyUp([key.Value]);;
        }
    }

    private void HandleCommandAction(ValueChangedEventArgs e)
    {
        if (!_commandMappings.TryGetValue(e.ControlId, out var action))
        {
            return;
        }

        switch (action)
        {
            case GamepadAction.Pause:
            case GamepadAction.TimeTravel:
                var args = new GamepadActionCommand(action, e.IsPressed ? InputState.Pressed : InputState.Released);
                _commandManager.SendCommand(args);

                break;
        }
    }
}
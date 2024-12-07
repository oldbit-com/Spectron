using System.Collections.ObjectModel;
using OldBit.Joypad;
using OldBit.Joypad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public record GamepadPreferences(
    Guid ControllerId,
    JoystickType JoystickType,
    List<GamepadMapping> InputMappings);

public sealed class GamepadManager
{
    private readonly JoypadManager _joypadManager;
    private readonly List<GamepadController> _controllers = [];

    private GamepadPreferences? _activeGamepad;
    private Dictionary<JoystickInput, List<GamepadMapping>> _joystickInputMappings = new();

    private bool _initialized;

    public IReadOnlyList<GamepadController> Controllers => _controllers;

    public event EventHandler<ControllerChangedEventArgs>? ControllerChanged;

    public GamepadManager()
    {
        _joypadManager = new JoypadManager();

        _joypadManager.ControllerConnected += OnControllerConnected;
        _joypadManager.ControllerDisconnected += OnControllerDisconnected;

        _controllers.Add(GamepadController.None);
        OnControllerChanged(GamepadController.None, ControllerChangedAction.Added);
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
    }

    public void Update()
    {
        if (_activeGamepad == null)
        {
            return;
        }

        _joypadManager.Update(_activeGamepad.ControllerId);
    }

    public void Update(Guid controllerId) => _joypadManager.Update(controllerId);

    public InputState GetInputState(JoystickInput input)
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

    private void OnControllerChanged(GamepadController controller, ControllerChangedAction action) =>
        ControllerChanged?.Invoke(this, new ControllerChangedEventArgs(controller, action));

    private void OnControllerConnected(object? sender, JoypadControllerEventArgs e)
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

        _controllers.Add(controller);
        OnControllerChanged(controller, ControllerChangedAction.Added);
    }

    private void OnControllerDisconnected(object? sender, JoypadControllerEventArgs e)
    {
        var existingController = _controllers.FirstOrDefault(x => x.ControllerId == e.Controller.Id);

        if (existingController != null)
        {
            _controllers.Remove(existingController);
            OnControllerChanged(existingController, ControllerChangedAction.Removed);
        }
    }
}
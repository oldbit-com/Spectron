using OldBit.Joypad;
using OldBit.Joypad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public class GamepadController
{
    private readonly JoypadController? _controller;

    public Guid ControllerId { get; private set; }

    public string Name { get; private set; }

    public IReadOnlyList<GamepadButton> Buttons { get; }

    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    public static GamepadController None { get; } = new();

    private GamepadController()
    {
        ControllerId = Guid.Empty;
        Name = "None";
        Buttons = [];
    }

    public GamepadController(JoypadController controller, IEnumerable<GamepadButton> buttons)
    {
        _controller = controller;

        ControllerId = controller.Id;
        Name = controller.Name;
        Buttons = buttons.ToList();

        _controller.ValueChanged += ControllerOnValueChanged;
    }

    private void ControllerOnValueChanged(object? sender, ControlEventArgs e)
    {
        var direction = DirectionalPadDirection.None;

        if (e.Control is { ControlType: ControlType.DirectionalPad, Value: not null })
        {
            direction = (DirectionalPadDirection)e.Control.Value;
        }

        ValueChanged?.Invoke(this, new ValueChangedEventArgs(e.Control.Id, e.Control.IsPressed, direction));
    }
}
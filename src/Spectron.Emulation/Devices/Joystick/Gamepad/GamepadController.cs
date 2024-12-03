using OldBit.JoyPad;
using OldBit.JoyPad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public class GamepadController
{
    private readonly JoyPadController? _controller;

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

    public GamepadController(JoyPadController controller, IEnumerable<GamepadButton> buttons)
    {
        _controller = controller;

        ControllerId = controller.Id;
        Name = controller.Name;
        Buttons = buttons.ToList();

        _controller.ValueChanged += ControllerOnValueChanged;
    }

    private void ControllerOnValueChanged(object? sender, ControlEventArgs e) =>
        ValueChanged?.Invoke(this, new ValueChangedEventArgs(e.Control.Id, e.Control.Value));
}
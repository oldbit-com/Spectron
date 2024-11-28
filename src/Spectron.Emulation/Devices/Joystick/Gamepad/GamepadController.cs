using OldBit.JoyPad;
using OldBit.JoyPad.Controls;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public class GamepadController
{
    private readonly JoyPadController? _controller;

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public IReadOnlyList<GamepadButton> Buttons { get; }

    public static GamepadController None { get; } = new();

    private GamepadController()
    {
        Id = Guid.Empty;
        Name = "None";
        Buttons = [];
    }

    public GamepadController(
        JoyPadController controller,
        IEnumerable<GamepadButton> buttons)
    {
        _controller = controller;

        Id = controller.Id;
        Name = controller.Name;
        Buttons = buttons.ToList();

        _controller.ValueChanged += ControllerOnValueChanged;
    }

    private void ControllerOnValueChanged(object? sender, ControlEventArgs e)
    {
        Console.WriteLine($"Value changed: {e.Control.Name} = {e.Control.Value}");
    }
}
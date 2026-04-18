using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.Joysticks;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Emulator.Tests.Devices.Joystick;

public class SinclairJoystickTests
{
    private readonly KeyboardState _keyboardState = new();

    [Theory]
    [InlineData(JoystickInput.Left, 0b1111_1110)]
    [InlineData(JoystickInput.Right, 0b1111_1101)]
    [InlineData(JoystickInput.Up, 0b1111_0111)]
    [InlineData(JoystickInput.Down, 0b1111_1011)]
    [InlineData(JoystickInput.Fire, 0b1110_1111)]
    public void Sinclair1JoystickButton_ShouldMapAsKey(JoystickInput input, byte expectedValue)
    {
        var joystick = new SinclairJoystick(_keyboardState, JoystickType.Sinclair1);

        joystick.HandleInput(input, InputState.Pressed);

        var result = _keyboardState.Read(0xF7FE);
        result.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(JoystickInput.Left, 0b1110_1111)]
    [InlineData(JoystickInput.Right, 0b1111_0111)]
    [InlineData(JoystickInput.Up, 0b1111_1101)]
    [InlineData(JoystickInput.Down, 0b1111_1011)]
    [InlineData(JoystickInput.Fire, 0b1111_1110)]
    public void Sinclair2JoystickButton_ShouldMapAsKey(JoystickInput input, byte expectedValue)
    {
        var joystick = new SinclairJoystick(_keyboardState, JoystickType.Sinclair2);

        joystick.HandleInput(input, InputState.Pressed);

        var result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void Sinclair1Joystick_ShouldMapMoreThanOneKey()
    {
        var joystick = new SinclairJoystick(_keyboardState, JoystickType.Sinclair1);

        joystick.HandleInput(JoystickInput.Up, InputState.Pressed);
        joystick.HandleInput(JoystickInput.Left, InputState.Pressed);
        joystick.HandleInput(JoystickInput.Fire, InputState.Pressed);

        joystick.HandleInput(JoystickInput.Right, InputState.Pressed);
        joystick.HandleInput(JoystickInput.Right, InputState.Released);

        var result = _keyboardState.Read(0xF7FE);
        result.ShouldBe(0b1110_0110);
    }

    [Fact]
    public void Sinclair2Joystick_ShouldMapMoreThanOneKey()
    {
        var joystick = new SinclairJoystick(_keyboardState, JoystickType.Sinclair2);

        joystick.HandleInput(JoystickInput.Up, InputState.Pressed);
        joystick.HandleInput(JoystickInput.Left, InputState.Pressed);
        joystick.HandleInput(JoystickInput.Fire, InputState.Pressed);

        joystick.HandleInput(JoystickInput.Right, InputState.Pressed);
        joystick.HandleInput(JoystickInput.Right, InputState.Released);

        var result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1110_1100);
    }
}
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.Joysticks;

namespace OldBit.Spectron.Emulator.Tests.Devices.Joystick;

public class FullerJoystickTests
{
    private readonly FullerJoystick _joystick = new();

    [Fact]
    public void FullerPort_ShouldRespond()
    {
        var result = _joystick.ReadPort(0x7F);
        result.ShouldBe(0xFF);
    }

    [Theory]
    [InlineData(JoystickInput.Right, InputState.Pressed, 0b1111_0111)]
    [InlineData(JoystickInput.Left, InputState.Pressed, 0b1111_1011)]
    [InlineData(JoystickInput.Down, InputState.Pressed, 0b1111_1101)]
    [InlineData(JoystickInput.Up, InputState.Pressed, 0b1111_1110)]
    [InlineData(JoystickInput.Fire, InputState.Pressed, 0b0111_1111)]
    public void FullerPort_ShouldReturnButtonValue(JoystickInput input, InputState state, byte expectedValue)
    {
        _joystick.HandleInput(input, state);

        var result = _joystick.ReadPort(0x7F);

        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void FullerPort_ShouldReturnMoreThanOneButtonValue()
    {
        _joystick.HandleInput(JoystickInput.Up, InputState.Pressed);
        _joystick.HandleInput(JoystickInput.Left, InputState.Pressed);
        _joystick.HandleInput(JoystickInput.Fire, InputState.Pressed);

        _joystick.HandleInput(JoystickInput.Right, InputState.Pressed);
        _joystick.HandleInput(JoystickInput.Right, InputState.Released);

        var result = _joystick.ReadPort(0x7F);

        result.ShouldBe(0b0111_1010);
    }
}
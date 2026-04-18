using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.Joysticks;

namespace OldBit.Spectron.Emulator.Tests.Devices.Joystick;

public class KempstonJoystickTests
{
    private readonly KempstonJoystick _joystick = new();

    [Theory]
    [InlineData(0x1F)]
    [InlineData(0x0F)]
    [InlineData(0x01)]
    [InlineData(0x00)]
    public void KempstonPort_ShouldRespond(Word port)
    {
        var result = _joystick.ReadPort(port);
        result.ShouldBe((byte)0);
    }

    [Theory]
    [InlineData(0x20)]
    [InlineData(0xFF)]
    public void KempstonPort_ShouldNotRespond(Word port)
    {
        var result = _joystick.ReadPort(port);
        result.ShouldBe(null);
    }

    [Theory]
    [InlineData(JoystickInput.Right, InputState.Pressed, 0x01)]
    [InlineData(JoystickInput.Left, InputState.Pressed, 0x02)]
    [InlineData(JoystickInput.Down, InputState.Pressed, 0x04)]
    [InlineData(JoystickInput.Up, InputState.Pressed, 0x08)]
    [InlineData(JoystickInput.Fire, InputState.Pressed, 0x10)]
    public void KempstonPort_ShouldReturnButtonValue(JoystickInput input, InputState state, byte expectedValue)
    {
        _joystick.HandleInput(input, state);

        var result = _joystick.ReadPort(0x1F);

        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void KempstonPort_ShouldReturnMoreThanOneButtonValue()
    {
        _joystick.HandleInput(JoystickInput.Up, InputState.Pressed);
        _joystick.HandleInput(JoystickInput.Left, InputState.Pressed);
        _joystick.HandleInput(JoystickInput.Fire, InputState.Pressed);

        _joystick.HandleInput(JoystickInput.Right, InputState.Pressed);
        _joystick.HandleInput(JoystickInput.Right, InputState.Released);

        var result = _joystick.ReadPort(0x1F);

        result.ShouldBe((byte)0x1A);
    }
}
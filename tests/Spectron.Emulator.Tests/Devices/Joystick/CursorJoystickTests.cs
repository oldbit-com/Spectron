using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.Joysticks;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Emulator.Tests.Devices.Joystick;

public class CursorJoystickTests
{
    private readonly KeyboardState _keyboardState = new();
    private readonly CursorJoystick _joystick;

    public CursorJoystickTests() => _joystick = new CursorJoystick(_keyboardState);

    [Fact]
    public void JoystickLeft_ShouldMapAsKey5()
    {
        _joystick.HandleInput(JoystickInput.Left, InputState.Pressed);

        var result = _keyboardState.Read(0xFEFE);
        result.ShouldBe(0b1111_1110);       // CAPS Shift

        result = _keyboardState.Read(0xF7FE);
        result.ShouldBe(0b1110_1111);       // 5 - Left
    }

    [Fact]
    public void JoystickRight_ShouldMapAsKey8()
    {
        _joystick.HandleInput(JoystickInput.Right, InputState.Pressed);

        var result = _keyboardState.Read(0xFEFE);
        result.ShouldBe(0b1111_1110);       // CAPS Shift

        result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1111_1011);       // 8 - Right
    }

    [Fact]
    public void JoystickUp_ShouldMapAsKey7()
    {
        _joystick.HandleInput(JoystickInput.Up, InputState.Pressed);

        var result = _keyboardState.Read(0xFEFE);
        result.ShouldBe(0b1111_1110);       // CAPS Shift

        result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1111_0111);       // 7 - Up
    }

    [Fact]
    public void JoystickDown_ShouldMapAsKey6()
    {
        _joystick.HandleInput(JoystickInput.Down, InputState.Pressed);

        var result = _keyboardState.Read(0xFEFE);
        result.ShouldBe(0b1111_1110);       // CAPS Shift

        result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1110_1111);       // 6 - Down
    }

    [Fact]
    public void JoystickFire_ShouldMapAsKey0()
    {
        _joystick.HandleInput(JoystickInput.Fire, InputState.Pressed);

        var result = _keyboardState.Read(0xFEFE);
        result.ShouldBe(0b1111_1110);       // CAPS Shift

        result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1111_1110);       // 0 - Fire
    }

    [Fact]
    public void Joystick_ShouldMapMoreThanOneKey()
    {
        _joystick.HandleInput(JoystickInput.Up, InputState.Pressed);
        _joystick.HandleInput(JoystickInput.Left, InputState.Pressed);
        _joystick.HandleInput(JoystickInput.Fire, InputState.Pressed);

        _joystick.HandleInput(JoystickInput.Right, InputState.Pressed);
        _joystick.HandleInput(JoystickInput.Right, InputState.Released);

        var result = _keyboardState.Read(0xFEFE);
        result.ShouldBe(0b1111_1111);       // CAPS Shift release

        result = _keyboardState.Read(0xF7FE);
        result.ShouldBe(0b1110_1111);       // 5 - Left

        result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1111_0110);       // 7 and 0 - Up and Fire
    }
}
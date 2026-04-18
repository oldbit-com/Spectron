using OldBit.Spectron.Emulation.Commands;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Emulator.Tests.Devices.Joystick;

public class JoystickManagerManagerTests
{
    private readonly KeyboardState _keyboardState = new();
    private readonly SpectrumBus _bus = new();
    private readonly GamepadManager _gamepadManager;
    private readonly CommandManager _commandManager = new();
    private readonly JoystickManager _joystickManager;

    public JoystickManagerManagerTests()
    {
        _gamepadManager = new GamepadManager(_keyboardState, _commandManager);
        _joystickManager = new JoystickManager(_gamepadManager, _bus, _keyboardState);
    }

    [Theory]
    [InlineData(JoystickInput.Right, 0x01)]
    [InlineData(JoystickInput.Left, 0x02)]
    [InlineData(JoystickInput.Down, 0x04)]
    [InlineData(JoystickInput.Up, 0x08)]
    [InlineData(JoystickInput.Fire, 0x10)]
    public void ShouldConfigure_KempstonJoystick(JoystickInput input, byte expectedValue)
    {
        _joystickManager.Configure(JoystickType.Kempston);
        _joystickManager.Pressed(input);

        var value = _bus.Read(0x1F);
        value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(JoystickInput.Right, 0b1111_0111)]
    [InlineData(JoystickInput.Left, 0b1111_1011)]
    [InlineData(JoystickInput.Down, 0b1111_1101)]
    [InlineData(JoystickInput.Up, 0b1111_1110)]
    [InlineData(JoystickInput.Fire, 0b0111_1111)]
    public void ShouldConfigure_FullerJoystick(JoystickInput input, byte expectedValue)
    {
        _joystickManager.Configure(JoystickType.Fuller);
        _joystickManager.Pressed(input);

        var value = _bus.Read(0x7F);
        value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(JoystickInput.Left, 0b1111_1110)]
    [InlineData(JoystickInput.Right, 0b1111_1101)]
    [InlineData(JoystickInput.Up, 0b1111_0111)]
    [InlineData(JoystickInput.Down, 0b1111_1011)]
    [InlineData(JoystickInput.Fire, 0b1110_1111)]
    public void ShouldConfigure_Sinclair1Joystick(JoystickInput input, byte expectedValue)
    {
        _joystickManager.Configure(JoystickType.Sinclair1);
        _joystickManager.Pressed(input);

        var result = _keyboardState.Read(0xF7FE);
        result.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(JoystickInput.Left, 0b1110_1111)]
    [InlineData(JoystickInput.Right, 0b1111_0111)]
    [InlineData(JoystickInput.Up, 0b1111_1101)]
    [InlineData(JoystickInput.Down, 0b1111_1011)]
    [InlineData(JoystickInput.Fire, 0b1111_1110)]
    public void ShouldConfigure_Sinclair2Joystick(JoystickInput input, byte expectedValue)
    {
        _joystickManager.Configure(JoystickType.Sinclair2);
        _joystickManager.Pressed(input);

        var result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void ShouldConfigure_CursorJoystick()
    {
        _joystickManager.Configure(JoystickType.Cursor);
        _joystickManager.Pressed(JoystickInput.Left);

        var result = _keyboardState.Read(0xF7FE);
        result.ShouldBe(0b1110_1111);       // 5 - Left

        _joystickManager.Released(JoystickInput.Left);
        _joystickManager.Pressed(JoystickInput.Right);

        result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1111_1011);       // 8 - Right

        _joystickManager.Released(JoystickInput.Right);
        _joystickManager.Pressed(JoystickInput.Up);

        result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1111_0111);       // 7 - Up

        _joystickManager.Released(JoystickInput.Up);
        _joystickManager.Pressed(JoystickInput.Down);

        result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1110_1111);       // 6 - Down

        _joystickManager.Released(JoystickInput.Down);
        _joystickManager.Pressed(JoystickInput.Fire);

        result = _keyboardState.Read(0xEFFE);
        result.ShouldBe(0b1111_1110);       // 0 - Fire
    }
}
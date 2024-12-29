namespace OldBit.Spectron.Emulation.Devices.Joystick;

internal interface IJoystick : IDevice
{
    void HandleInput(JoystickInput input, InputState state);
}
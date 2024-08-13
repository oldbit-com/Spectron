namespace OldBit.Spectral.Emulation.Devices.Joystick;

internal interface IJoystick : IDevice
{
    void HandleInput(JoystickInput input, bool isOn);
}
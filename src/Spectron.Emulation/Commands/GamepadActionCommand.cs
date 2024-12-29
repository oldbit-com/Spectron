using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

namespace OldBit.Spectron.Emulation.Commands;

public record GamepadActionCommand(GamepadAction Action, InputState State) : ICommand;
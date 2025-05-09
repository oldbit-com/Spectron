using OldBit.Spectron.Emulation.Devices.Gamepad;
using OldBit.Spectron.Emulation.Devices.Joystick;

namespace OldBit.Spectron.Emulation.Commands;

public record GamepadActionCommand(GamepadAction Action, InputState State) : ICommand;
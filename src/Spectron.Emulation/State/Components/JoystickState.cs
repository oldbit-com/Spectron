using MemoryPack;
using OldBit.Spectron.Emulation.Devices.Joystick;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record JoystickState
{
    public JoystickType JoystickType { get; set; }
}
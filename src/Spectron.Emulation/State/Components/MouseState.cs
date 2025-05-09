using MemoryPack;
using OldBit.Spectron.Emulation.Devices.Mouse;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record MouseState
{
    public MouseType MouseType { get; set; }
}
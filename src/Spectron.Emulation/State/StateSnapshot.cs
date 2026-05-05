using MemoryPack;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.State.Components;

namespace OldBit.Spectron.Emulation.State;

[MemoryPackable]
public sealed partial class StateSnapshot
{
    public ComputerType ComputerType { get; set; }

    public Color BorderColor { get; set; }

    public CpuState Cpu { get; set; } = new();

    public MemoryState Memory { get; set; } = new();

    public AyState? Ay { get; set; }

    public UlaPlusState? UlaPlus { get; set; }

    public CustomRomState? CustomRom { get; set; }

    public JoystickState Joystick { get; set; } = new();

    public MouseState Mouse { get; set; } = new();

    public TapeState? Tape { get; set; }

    public DivMmcState? DivMmc { get; set; }

    public Interface1State? Interface1 { get; set; }

    public Beta128State? Beta128 { get; set; }

    public PrinterState? Printer { get; set; }

    public TimexState? Timex { get; set; }

    public byte[] Serialize() => MemoryPackSerializer.Serialize(this);

    public static StateSnapshot? Deserialize(byte[] data) => MemoryPackSerializer.Deserialize<StateSnapshot>(data);
}
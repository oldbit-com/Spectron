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

    public byte[] Serialize() => MemoryPackSerializer.Serialize(this);

    public static StateSnapshot? Load(string filePath)
    {
        var file = File.ReadAllBytes(filePath);

        return MemoryPackSerializer.Deserialize<StateSnapshot>(file);
    }

    public static StateSnapshot? Deserialize(byte[] data) =>
        MemoryPackSerializer.Deserialize<StateSnapshot>(data);

    public static StateSnapshot? Load(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return MemoryPackSerializer.Deserialize<StateSnapshot>(memoryStream.ToArray());
    }

    public void Save(string fileName)
    {
        using var stream = File.Create(fileName);

        var data = Serialize();

        stream.Write(data, 0, data.Length);
        stream.Close();
    }
}
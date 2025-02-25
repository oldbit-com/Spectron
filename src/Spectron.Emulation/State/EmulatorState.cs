using MemoryPack;
using OldBit.Spectron.Emulation.Screen;
using OldBit.Spectron.Emulation.State.Components;

namespace OldBit.Spectron.Emulation.State;

[MemoryPackable]
public sealed partial class EmulatorState
{
    public ComputerType ComputerType { get; set; }

    public Color BorderColor { get; set; }

    public CpuState Cpu { get; set; } = new();

    public MemoryState Memory { get; set; } = new();

    public AyState? Ay { get; set; }

    public UlaPlusState? UlaPlus { get; set; }

    public CustomRomState? CustomRom { get; set; }

    public JoystickState Joystick { get; set; } = new();

    public TapeState? Tape { get; set; }

    public byte[] Serialize() => MemoryPackSerializer.Serialize(this);

    public static EmulatorState? Load(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return MemoryPackSerializer.Deserialize<EmulatorState>(memoryStream.ToArray());
    }

    internal void Save(string fileName)
    {
        using var stream = File.Create(fileName);

        var data = Serialize();

        stream.Write(data, 0, data.Length);
        stream.Close();
    }
}
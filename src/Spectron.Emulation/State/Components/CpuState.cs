using MemoryPack;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record CpuState
{
    public Word AF { get; set; }

    public Word BC { get; set; }

    public Word DE { get; set; }

    public Word HL { get; set; }

    public Word IX { get; set; }

    public Word IY { get; set; }

    public Word PC { get; set; }

    public Word SP { get; set; }

    public byte I { get; set; }

    public byte R { get; set; }

    public Word AFPrime { get; set; }

    public Word BCPrime { get; set; }

    public Word DEPrime { get; set; }

    public Word HLPrime { get; set; }

    public bool IsHalted { get; set; }

    public bool IFF1 { get; set; }

    public bool IFF2 { get; set; }

    public InterruptMode IM { get; set; }
}
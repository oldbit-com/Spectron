using OldBit.Spectron.Emulation.Devices.Contention;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation;

public sealed class EmulatorClock : Clock
{
    private readonly int _ticksPerFrame;

    public EmulatorClock(int ticksPerFrame, int multiplier = 1)
    {
        _ticksPerFrame = ticksPerFrame;
        Multiplier = multiplier;

        TicksPerFrame = _ticksPerFrame * multiplier;
    }

    public int Multiplier
    {
        get;
        set
        {
            field = value;
            TicksPerFrame = _ticksPerFrame * value;

            (ContentionProvider as ContentionProvider48K)?.ClockMultiplier = value;
            (ContentionProvider as ContentionProvider128K)?.ClockMultiplier = value;
        }
    }

    internal int TicksPerFrame { get; private set; }
    internal int UlaTicks => FrameTicks / Multiplier;
}
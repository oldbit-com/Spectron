using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation;

public sealed class EmulatorClock(int ticksPerFrame, Clock clock, int multiplier = 1)
{
    public int Multiplier
    {
        get;
        set
        {
            field = value;
            TicksPerFrame = ticksPerFrame * Multiplier;
        }
    } = multiplier;

    internal int TicksPerFrame = ticksPerFrame * multiplier;
    internal int FrameTicks => clock.FrameTicks * Multiplier;
    internal int UlaTicks => clock.FrameTicks / Multiplier;
}
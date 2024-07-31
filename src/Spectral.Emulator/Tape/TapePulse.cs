namespace OldBit.Spectral.Emulator.Tape;

internal record TapePulse(int PulseCount, int PulseLength)
{
    internal IEnumerable<TapePulse> AsEnumerable()
    {
        yield return this;
    }
}
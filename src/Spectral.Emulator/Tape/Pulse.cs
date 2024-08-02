namespace OldBit.Spectral.Emulator.Tape;

internal record Pulse(int RepeatCount, int Duration)
{
    internal IEnumerable<Pulse> AsEnumerable()
    {
        yield return this;
    }
}
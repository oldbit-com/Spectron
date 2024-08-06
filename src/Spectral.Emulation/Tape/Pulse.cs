namespace OldBit.Spectral.Emulation.Tape;

internal record Pulse(int RepeatCount, int Duration)
{
    internal IEnumerable<Pulse> AsEnumerable()
    {
        yield return this;
    }
}
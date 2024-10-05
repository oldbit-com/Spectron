namespace OldBit.Spectron.Emulation.Devices.Audio;

internal record BeeperState(int Ticks, byte Ear)
{
    public int Ticks { get; set; } = Ticks;
    public byte Ear { get; set; } = Ear;
}

/// <summary>
/// Provides a reusable buffer for beeper states. Once the buffer is primed, it will start reusing existing records.
/// This way it is memory efficient and avoids creating new instances when we dont need to.
/// </summary>
internal sealed class BeeperStates
{
    private readonly List<BeeperState> _beeperStates = [];

    internal int Count { get; private set; }
    internal BeeperState this[int index] => _beeperStates[index];

    internal void Add(int ticks, byte ear)
    {
        // If the last state has the same value, simply extend the ticks
        if (Count > 0)
        {
            var lastState = _beeperStates[Count - 1];

            if (lastState.Ear == ear)
            {
                lastState.Ticks = ticks;

                return;
            }
        }

        if (_beeperStates.Count > Count)
        {
            // Reuse record to avoid creating new instances
            _beeperStates[Count].Ticks = ticks;
            _beeperStates[Count].Ear = ear;
        }
        else
        {
            // Create new record
            _beeperStates.Add(new BeeperState(ticks, ear));
        }

        Count += 1;
    }

    public void Reset() => Count = 0;
}
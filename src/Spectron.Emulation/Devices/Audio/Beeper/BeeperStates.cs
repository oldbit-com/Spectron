namespace OldBit.Spectron.Emulation.Devices.Audio.Beeper;

internal record BeeperState(int Ticks, byte EarMic)
{
    public int Ticks { get; set; } = Ticks;
    public byte EarMic { get; set; } = EarMic;
}

/// <summary>
/// Provides a reusable buffer for beeper states. Once the buffer is primed, it will start reusing existing records.
/// This way it is memory efficient and avoids creating new instances when we don't need to. Beeper uses a lot of states.
/// </summary>
internal sealed class BeeperStates
{
    private readonly List<BeeperState> _beeperStates = [];

    internal int Count { get; private set; }
    internal BeeperState this[int index] => _beeperStates[index];

    internal void Add(int ticks, byte earMic)
    {
        // If the last state has the same value, extend the last state ticks
        if (Count > 0)
        {
            var lastState = _beeperStates[Count - 1];

            if (lastState.EarMic == earMic)
            {
                lastState.Ticks = ticks;

                return;
            }
        }

        if (_beeperStates.Count > Count)
        {
            _beeperStates[Count].Ticks = ticks;
            _beeperStates[Count].EarMic = earMic;
        }
        else
        {
            _beeperStates.Add(new BeeperState(ticks, earMic));
        }

        Count += 1;
    }

    public void Reset() => Count = 0;
}
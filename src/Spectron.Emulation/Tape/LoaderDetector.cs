namespace OldBit.Spectron.Emulation.Tape;

/// <summary>
/// Detects the loading process during tape emulation by monitoring the timing of data reads.
/// This is used to detect if there is a loader code executing so we can start cassette player.
/// </summary>
internal sealed class LoaderDetector
{
    private const int MaxTicksBetweenReads = 60;
    private const int MaxReadCount = 50;

    private int _lastTicks;
    private int _counter;

    internal bool Process(int ticks)
    {
        if (ticks - _lastTicks is > 0 and < MaxTicksBetweenReads)
        {
            _counter += 1;
        }
        else
        {
            _counter = 0;
        }

        if (_counter > MaxReadCount)
        {
            _counter = 0;
            return true;
        }

        _lastTicks = ticks;

        return false;
    }
}
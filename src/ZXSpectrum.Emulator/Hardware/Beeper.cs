using OldBit.Beep;

namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Beeper
{
    private const int CyclesPerSample = 16;
    private const int MaxBeepCycles = 214042;     // BEEP x,-60

    private int _lastEarValue;
    private long _lastTotalCycles;
    private byte _amplitude;
    private readonly AudioPlayer _audioPlayer;
    private readonly BeeperBuffer _beeperBuffer;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Beeper(float clockMHz)
    {
        var sampleRate = clockMHz * 1000000 / CyclesPerSample;
        _audioPlayer = new AudioPlayer(AudioFormat.Unsigned8Bit, (int)sampleRate, 1);

        _beeperBuffer = new BeeperBuffer(16384 * 8, () => _amplitude);

        Start();
    }

    public void UpdateBeeper(byte value, long totalCycles)
    {
        var earValue = value & 0x10;
        if (earValue == _lastEarValue)
        {
            return;
        }

        _lastEarValue = earValue;
        var cycles = totalCycles - _lastTotalCycles;
        _lastTotalCycles = totalCycles;

        if (cycles is 0 or > MaxBeepCycles)
        {
            return;
        }

        _amplitude = _amplitude == 0 ? (byte)0xFF : (byte)0;
        var length = (int)(cycles / CyclesPerSample);

        WriteBuffer(_amplitude, length);
    }

    private void Start()
    {
        _audioPlayer.Start();

        Task.Run(async () =>
        {
            await _audioPlayer.PlayAsync(_beeperBuffer);
        }, _cancellationTokenSource.Token);
    }

    private void WriteBuffer(byte amplitude, int length)
    {
        _beeperBuffer.Write(Enumerable.Repeat(amplitude, length).ToArray());
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }
}

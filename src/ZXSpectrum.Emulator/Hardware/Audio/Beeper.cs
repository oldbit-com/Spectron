using OldBit.Beep;

namespace OldBit.ZXSpectrum.Emulator.Hardware.Audio;

public class Beeper
{
    private const int StatesPerSample = 8;
    private const int MaxBeepStates = 214042;     // BEEP x,-60

    private int _lastEarValue;
    private long _lastTotalStates;
    private byte _amplitude;
    private readonly AudioPlayer _audioPlayer;
    private readonly BeeperBuffer _beeperBuffer;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Beeper(float clockMHz)
    {
        var sampleRate = clockMHz * 1000000 / StatesPerSample;
        _audioPlayer = new AudioPlayer(AudioFormat.Unsigned8Bit, (int)sampleRate, 1);

        _beeperBuffer = new BeeperBuffer(16384 * 8, () => _amplitude);

        Start();
    }

    public void UpdateBeeper(byte value, long totalStates)
    {
        var earValue = value & 0x10;
        if (earValue == _lastEarValue)
        {
            return;
        }

        _lastEarValue = earValue;
        var states = totalStates - _lastTotalStates;
        _lastTotalStates = totalStates;

        if (states is 0 or > MaxBeepStates)
        {
            return;
        }

        _amplitude = _amplitude == 0 ? (byte)0xFF : (byte)0;
        var length = (int)(states / StatesPerSample);

        WriteBuffer(_amplitude, length);
    }

    private void Start()
    {
        _audioPlayer.Start();

        Task.Run(async () =>
        {
            await _audioPlayer.PlayAsync(_beeperBuffer, _cancellationTokenSource.Token);
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

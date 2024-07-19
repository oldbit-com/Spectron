using System.Diagnostics;
using OldBit.Beep;

namespace OldBit.ZXSpectrum.Emulator.Hardware.Audio;

public class Beeper
{
    private const int PlayerSampleRate = 44100;
    private const int MaxBeepTicks = 214042;     // BEEP x,-60 - assume the longest lasting note

    private const byte LowAmplitude = 0x40;
    private const byte HighAmplitude = 0xBF;

    private int _lastEar;
    private long _lastTicks;
    private float _remainingTicks;
    private byte _amplitude = LowAmplitude;
    private readonly AudioPlayer _audioPlayer;
    private readonly BeeperBuffer _beeperBuffer;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly float _statesPerSample;

    public Beeper(float clockMHz)
    {
        _statesPerSample = clockMHz * 1_000_000 / PlayerSampleRate;

        _audioPlayer = new AudioPlayer(AudioFormat.Unsigned8Bit, PlayerSampleRate, 1);
        _beeperBuffer = new BeeperBuffer(16384*4, () => _amplitude);

        Start();
    }

    public void UpdateBeeper(byte value, long ticks)
    {
        var ear = value & 0x10;
        if (ear == _lastEar)
        {
            return;
        }

        var ticksElapsed = ticks - _lastTicks;
        if (ticksElapsed > MaxBeepTicks)
        {
            // This when there was no real beeper event for some time, no real sound was generated
            ticksElapsed = 0;
        }

        _remainingTicks += ticksElapsed;

        var samples = (int)(_remainingTicks / _statesPerSample);
        if (samples > 0)
        {
            WriteBuffer(_amplitude, samples);
            _remainingTicks -= samples * _statesPerSample;
        }
        else
        {
            Debug.WriteLine($"Clipping");
        }

        _amplitude = _amplitude == LowAmplitude ? HighAmplitude : LowAmplitude;
        _lastEar = ear;
        _lastTicks = ticks;
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

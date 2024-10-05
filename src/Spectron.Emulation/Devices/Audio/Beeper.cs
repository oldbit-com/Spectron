using OldBit.Beep;

namespace OldBit.Spectron.Emulation.Devices.Audio;

internal sealed class Beeper
{
    private const int PlayerSampleRate = 44100;
    // TODO: Maybe use long instead of int?
    private const int Multiplier = 1000;         // Used to avoid floating point arithmetic and rounding errors
    private const int FramesPerSecond = 50;
    private const int SamplesPerFrame = PlayerSampleRate / FramesPerSecond;

    private const short LowAmplitude = -32000;
    private const short HighAmplitude = 0;

    private int _lastEar;
    private bool _isMuted;
    private AudioPlayer? _audioPlayer;
    private bool _isAudioPlayerRunning;

    private readonly int _statesPerSample;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly BeeperStates _beeperStates = new();

    private readonly byte[] _buffer = new byte[SamplesPerFrame * 2];

    internal Beeper(HardwareSettings hardware)
    {
        var ticksPerFrame = hardware.TicksPerFrame;
        _statesPerSample = Multiplier * ticksPerFrame / SamplesPerFrame;
    }

    internal void EndFrame(int frameTicks)
    {
        if (!_isAudioPlayerRunning || _isMuted)
        {
            _beeperStates.Reset();
            return;
        }


        var runningTicks = 0;
        var bufferIndex = 0;
        // Array.Fill(_buffer, _lastEar);

        var buffer = new List<byte>();

        var duration = 0;
        var remainingTicks = 0;

        if (_beeperStates.Count == 0)
        {
            duration = frameTicks * Multiplier;
        }
        else
        {
            duration = _beeperStates[0].Ticks * Multiplier;
        }

        for (var i = 0; i <= _beeperStates.Count; i++)
        {
            var sample = _lastEar != 0 ? HighAmplitude : LowAmplitude;

            runningTicks += duration;
            duration += remainingTicks;


            while (duration >= _statesPerSample)
            {
                duration -= _statesPerSample;

                buffer.Add((byte)sample);
                buffer.Add((byte)(sample >> 8));
                // _buffer[bufferIndex++] = (byte)sample;
                // _buffer[bufferIndex++] = (byte)(sample >> 8);
            }

            remainingTicks = duration;

            if (i == _beeperStates.Count)
            {
                break;
            }

            _lastEar = _beeperStates[i].Ear;

            if (i == _beeperStates.Count - 1)
            {
                duration = frameTicks * Multiplier - runningTicks;
            }
            else
            {
                duration = _beeperStates[i + 1].Ticks * Multiplier - runningTicks;
            }
        }

        _audioPlayer?.TryEnqueue(buffer);

        _beeperStates.Reset();
    }

    internal void UpdateBeeper(int frameTicks, byte value)
    {
        if (_isMuted)
        {
            return;
        }

        var ear = (byte)(value & 0x10);
        _beeperStates.Add(frameTicks, ear);
    }

    internal void Reset()
    {
        _beeperStates.Reset();;
    }

    internal void Start()
    {
        _audioPlayer = new AudioPlayer(
            AudioFormat.Signed16BitIntegerLittleEndian,
            PlayerSampleRate,
            channelCount: 1,
            new PlayerOptions
            {
                BufferSizeInBytes = 16384
            });

        _audioPlayer.AddFilter(new BeeperFilter());

        _audioPlayer.Volume = 50;
        _audioPlayer.Start();
        _isAudioPlayerRunning = true;
    }

    internal void Stop()
    {
        _isAudioPlayerRunning = false;
        _audioPlayer?.Stop();

        _audioPlayer?.Dispose();
        _cancellationTokenSource.Dispose();
    }

    internal void Mute() => _isMuted = true;

    internal void UnMute() => _isMuted = false;
}

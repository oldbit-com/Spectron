using OldBit.Beep;

namespace OldBit.Spectron.Emulation.Devices.Audio;

internal record BeeperState(int Ticks, byte Ear)
{
    public int Ticks { get; set; } = Ticks;
}

internal sealed class Beeper
{
    private const int PlayerSampleRate = 44100;
    private const int Multiplier = 100;         // Used to avoid floating point arithmetic and rounding errors
    private const int FramesPerSecond = 50;
    private const int SamplesPerFrame = PlayerSampleRate / FramesPerSecond;

    private const short LowAmplitude = -32000;
    private const short HighAmplitude = 0;

    private byte _lastEar;
    private bool _isMuted;
    private readonly int _ticksPerFrame;
    private readonly int _statesPerSample;

    private AudioPlayer? _audioPlayer;
    private bool _isAudioPlayerRunning;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List<BeeperState> _beeperStates = [];

    internal Beeper(HardwareSettings hardware)
    {
        _ticksPerFrame = hardware.TicksPerFrame;
        _statesPerSample = Multiplier * _ticksPerFrame / SamplesPerFrame;
    }

    internal void EndFrame()
    {
        if (!_isAudioPlayerRunning || _isMuted)
        {
            _beeperStates.Clear();
            return;
        }

        UpdateBeeper(_ticksPerFrame, _lastEar);

        var buffer = new List<byte>();
        var remainingTicks = 0;
        var runningTicks = 0;

        foreach (var state in _beeperStates)
        {
            var duration = state.Ticks * Multiplier - runningTicks;
            runningTicks += duration;
            duration += remainingTicks;

            while (duration >= _statesPerSample)
            {
                duration -= _statesPerSample;

                var sample = state.Ear != 0 ? HighAmplitude : LowAmplitude;
                buffer.AddRange(BitConverter.GetBytes(sample));
            }

            remainingTicks = duration;
        }

        _audioPlayer?.TryEnqueue(buffer);

        _beeperStates.Clear();
    }

    internal void UpdateBeeper(int ticks, byte value)
    {
        if (_isMuted)
        {
            return;
        }

        var ear = (byte)(value & 0x10);
        _lastEar = ear;

        if (_beeperStates.Count > 0)
        {
            var lastState = _beeperStates[^1];
            if (lastState.Ear == ear)
            {
                lastState.Ticks = ticks;
                return;
            }
        }

        _beeperStates.Add(new BeeperState(ticks, ear));
    }

    internal void Reset()
    {
        _beeperStates.Clear();
        _lastEar = 0;
    }

    internal void Start()
    {
        _audioPlayer = new AudioPlayer(
            AudioFormat.Signed16BitIntegerLittleEndian,
            PlayerSampleRate,
            channelCount: 1,
            new PlayerOptions { BufferSizeInBytes = 16384, MaxBuffers = 5 });

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

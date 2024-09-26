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

    private const byte LowAmplitude = 0x40;
    private const byte HighAmplitude = 0xBF;

    private byte _lastEar;
    private readonly int _ticksPerFrame;
    private readonly int _statesPerSample;

    private AudioPlayer? _audioPlayer;
    private readonly BeeperBuffer _beeperBuffer;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly List<BeeperState> _beeperStates = [];

    internal Beeper(HardwareSettings hardware)
    {
        _ticksPerFrame = hardware.TicksPerFrame;
        _statesPerSample = Multiplier * _ticksPerFrame / SamplesPerFrame;
        _beeperBuffer = new BeeperBuffer(16384 * 4, () => LowAmplitude);
    }

    internal void EndFrame()
    {
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
                buffer.Add(state.Ear != 0 ? LowAmplitude : HighAmplitude);
            }

            remainingTicks = duration;
        }

        _beeperBuffer.Write(buffer.ToArray());
        _beeperStates.Clear();
    }

    internal void UpdateBeeper(int ticks, byte value)
    {
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

    internal void Start()
    {
        _audioPlayer = new AudioPlayer(
            AudioFormat.Unsigned8Bit,
            PlayerSampleRate,
            channelCount: 1,
            new PlayerOptions { BufferSizeInBytes = 2048 });

        Task.Run(async () =>
        {
            try
            {
                await _audioPlayer.PlayAsync(_beeperBuffer, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException e)
            {
                _audioPlayer.Dispose();
            }
        }, _cancellationTokenSource.Token);
    }

    internal void Stop() => _cancellationTokenSource.Cancel();
}

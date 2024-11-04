using OldBit.Beep;
using OldBit.Spectron.Emulation.Devices.Audio.AY;
using OldBit.Spectron.Emulation.Devices.Audio.Beeper;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Audio;

public sealed class AudioManager
{
    private const int FramesPerSecond = 50;
    private const int PlayerSampleRate = 44100;
    private const int SamplesPerFrame = PlayerSampleRate / FramesPerSecond;
    private const int NumberOfBuffers = 4;

    private readonly bool _hasAyChip;
    private readonly AudioBufferPool _audioBufferPool;
    private readonly BeeperAudio _beeperAudio;
    private readonly AyAudio _ayAudio;

    private bool _isMuted;
    private bool _isAyEnabled;
    private bool _isBeeperEnabled;
    private AudioPlayer? _audioPlayer;
    private bool _isAudioPlayerRunning;

    internal BeeperDevice Beeper { get; }
    internal AyDevice Ay { get; } = new();

    public bool IsAySupported => _hasAyChip || IsAySupportedStandardSpectrum;

    public bool IsBeeperEnabled
    {
        get => _isBeeperEnabled;
        set
        {
            if (_isBeeperEnabled == value)
            {
                return;
            }

            _isBeeperEnabled = value;
            Beeper.IsEnabled = value;

            ToggleAudioPlayer();
        }
    }

    public bool IsAyEnabled
    {
        get => _isAyEnabled;
        set
        {
            if (_isAyEnabled == value)
            {
                return;
            }

            _isAyEnabled = value;
            Ay.IsEnabled = value;

            ToggleAudioPlayer();
        }
    }

    public bool IsAySupportedStandardSpectrum { get; set; } = true;

    internal AudioManager(Clock clock, CassettePlayer? cassettePlayer, HardwareSettings hardware)
    {
        _hasAyChip = hardware.HasAyChip;
        var statesPerSample = (double)hardware.TicksPerFrame / SamplesPerFrame;

        _beeperAudio = new BeeperAudio(clock, statesPerSample);
        Beeper = new BeeperDevice(cassettePlayer)
        {
            OnUpdateBeeper = _beeperAudio.Update
        };

        _ayAudio = new AyAudio(clock, Ay, statesPerSample);
        _audioBufferPool = new AudioBufferPool(NumberOfBuffers);
    }

    internal void NewFrame()
    {
        _beeperAudio.NewFrame();
        _ayAudio.NewFrame();
    }

    internal void EndFrame()
    {
        if (_isMuted)
        {
            return;
        }

        var playAudio = false;
        if (IsAySupported && _isAyEnabled)
        {
            _ayAudio.EndFrame();
            playAudio = true;
        }

        if (_isBeeperEnabled)
        {
            _beeperAudio.EndFrame();
            playAudio = true;
        }

        if (!playAudio)
        {
            return;
        }

        var audioBuffer = _audioBufferPool.GetBuffer();
        var samplesCount = _beeperAudio.Samples.Count == 0 ? Ay.ChannelA.Samples.Count : _beeperAudio.Samples.Count;

        for (var i = 0; i < samplesCount; i++)
        {
            var sample = _beeperAudio.Samples.Count > i ? _beeperAudio.Samples[i] : 0;
            if (Ay.ChannelA.Samples.Count > i)
            {
                sample += Ay.ChannelA.Samples[i] + Ay.ChannelB.Samples[i] + Ay.ChannelC.Samples[i];
            }
            audioBuffer.Add((short)sample);
        }

        _audioPlayer?.TryEnqueue(audioBuffer.Buffer);
    }

    internal void Start()
    {
        if (_isAudioPlayerRunning)
        {
            return;
        }

        _audioPlayer = new AudioPlayer(
            AudioFormat.Signed16BitIntegerLittleEndian,
            PlayerSampleRate,
            channelCount: 1,
            new PlayerOptions
            {
                BufferSizeInBytes = 32768,
                BufferQueueSize = NumberOfBuffers,
            });

        _audioPlayer.Volume = 80;
        _audioPlayer.Start();
        _isAudioPlayerRunning = true;
    }

    internal void Stop()
    {
        _beeperAudio.Stop();

        _isAudioPlayerRunning = false;
        _audioPlayer?.Stop();

        _audioPlayer?.Dispose();
    }

    internal void ResetAudio()
    {
        _beeperAudio.Reset();
        _ayAudio.Reset();
    }

    public void Mute() => _isMuted = true;

    public void UnMute() => _isMuted = false;

    private void ToggleAudioPlayer()
    {
        if (Beeper.IsEnabled || Ay.IsEnabled)
        {
            Start();
        }
        else
        {
            Stop();
        }
    }
}
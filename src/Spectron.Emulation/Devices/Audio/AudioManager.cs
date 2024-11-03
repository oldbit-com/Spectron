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
    private bool _isAyAudioEnabled;
    private AudioPlayer? _audioPlayer;
    private bool _isAudioPlayerRunning;

    internal BeeperDevice Beeper { get; }
    internal AyDevice Ay { get; } = new();

    public bool IsBeeperEnabled
    {
        get => _beeperAudio.IsEnabled;
        set
        {
            if (_beeperAudio.IsEnabled == value) return;
            ToggleBeeperEnabled(value);
        }
    }

    public bool IsAySupported => _hasAyChip || IsAyAudioEnabled48K;

    public bool IsAyAudioEnabled
    {
        get => _isAyAudioEnabled;
        set
        {
            if (_isAyAudioEnabled == value)
            {
                return;
            }

            _isAyAudioEnabled = value;
            Ay.IsEnabled = value;
        }
    }

    public bool IsAyAudioEnabled48K { get; set; } = true;

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
        _beeperAudio.Samples.Clear();
    }

    internal void EndFrame()
    {
        _ayAudio.EndFrame();
        _beeperAudio.EndFrame();

        var samples = _beeperAudio.Samples;
        var audioBuffer = _audioBufferPool.GetBuffer();

        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];

            if (_isAyAudioEnabled)
            {
                sample += Ay.ChannelA.Samples[i] + Ay.ChannelB.Samples[i] + Ay.ChannelC.Samples[i];
            }

            audioBuffer.Add((short)sample);
        }

        if (_beeperAudio.IsEnabled && !_isMuted)
        {
            _audioPlayer?.TryEnqueue(audioBuffer.Buffer);
        }
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

    public void Mute()
    {
        _isMuted = true;
        _beeperAudio.Mute();
    }

    public void UnMute()
    {
        _isMuted = false;
        _beeperAudio.UnMute();
    }

    private void ToggleBeeperEnabled(bool isEnabled)
    {
        _beeperAudio.IsEnabled = isEnabled;

        if (isEnabled)
        {
            Start();
        }
        else
        {
            Stop();
        }
    }
}
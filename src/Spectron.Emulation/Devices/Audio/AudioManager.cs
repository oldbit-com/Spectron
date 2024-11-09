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

    private StereoMode _stereoMode = StereoMode.None;
    private bool _isMuted;
    private bool _isAyEnabled;
    private bool _isBeeperEnabled;
    private AudioPlayer? _audioPlayer;
    private bool _isAudioPlayerRunning;

    internal BeeperDevice Beeper { get; }
    internal AyDevice Ay { get; } = new();

    public StereoMode StereoMode
    {
        get => _stereoMode;
        set
        {
            if (_stereoMode == value)
            {
                return;
            }

            _stereoMode = value;

            ConfigureStereoMode();
        }
    }

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
            var sampleL = sample;
            var sampleR = sample;

            if (Ay.ChannelA.Samples.Count > i)
            {
                var center = 0;

                switch (StereoMode)
                {
                    case StereoMode.None:
                        sample += Ay.ChannelA.Samples[i] + Ay.ChannelB.Samples[i] + Ay.ChannelC.Samples[i];
                        break;

                    case StereoMode.StereoAbc:
                        center = (int)(Ay.ChannelB.Samples[i] * 0.7);
                        sampleL += Ay.ChannelA.Samples[i] + center + (int)(Ay.ChannelC.Samples[i] * 0.3);
                        sampleR += (int)(Ay.ChannelA.Samples[i] * 0.3) + center + Ay.ChannelC.Samples[i];
                        break;

                    case StereoMode.StereoAcb:
                        center = (int)(Ay.ChannelC.Samples[i] * 0.7);
                        sampleL += Ay.ChannelA.Samples[i] + center + (int)(Ay.ChannelB.Samples[i] * 0.3);
                        sampleR += (int)(Ay.ChannelA.Samples[i] * 0.3) + center + Ay.ChannelB.Samples[i];
                        break;
                }
            }

            if (StereoMode == StereoMode.None)
            {
                audioBuffer.Add((short)sample);
            }
            else
            {
                audioBuffer.Add((short)sampleL);
                audioBuffer.Add((short)sampleR);
            }
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
            channelCount: StereoMode == StereoMode.None ? 1 : 2,
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

    private void ConfigureStereoMode()
    {
        Stop();
        Start();
    }
}
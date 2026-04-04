using OldBit.Beep;
using OldBit.Spectron.Emulation.Devices.Audio.AY;
using OldBit.Spectron.Emulation.Devices.Audio.Beeper;
using OldBit.Spectron.Emulation.Devices.Timex;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Audio;

public sealed class AudioManager
{
    public const int PlayerSampleRate = 44100;

    private const AudioFormat PlayerAudioFormat = AudioFormat.Signed16BitIntegerLittleEndian;
    private const int FramesPerSecond = 50;
    private const int SamplesPerFrame = PlayerSampleRate / FramesPerSecond;
    private const int NumberOfBuffers = 4;

    private readonly AudioBufferPool _audioBufferPool;
    private readonly BeeperAudio _beeperAudio;
    private readonly AyAudio _ayAudio;

    private bool _isMuted;
    private AudioPlayer? _audioPlayer;
    private bool _isAudioPlayerRunning;

    internal BeeperDevice Beeper { get; }
    internal AyDevice Ay { get; }

    public StereoMode StereoMode
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;

            Restart();
        }
    } = StereoMode.Mono;

    public bool IsAySupported => field || IsAySupportedStandardSpectrum;

    public bool IsBeeperEnabled
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            Beeper.IsEnabled = value;

            ToggleAudioPlayer();
        }
    }

    public bool IsAyEnabled
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            Ay.IsEnabled = value;

            ToggleAudioPlayer();
        }
    }

    public bool IsAySupportedStandardSpectrum { get; set; } = true;

    internal AudioManager(Clock clock, CassettePlayer? cassettePlayer, HardwareSettings hardware, Func<Word, bool> isUlaPort)
    {
        Ay = hardware.ComputerType == ComputerType.Timex2048 ? new AyTimexDevice() : new AyDevice();

        IsAySupported = hardware.HasAyChip;

        var statesPerSample = (double)hardware.TicksPerFrame / SamplesPerFrame;

        _beeperAudio = new BeeperAudio(clock, statesPerSample, hardware.ClockMhz);

        Beeper = new BeeperDevice(cassettePlayer, isUlaPort)
        {
            OnUpdateBeeper = _beeperAudio.Update
        };

        _ayAudio = new AyAudio(clock, Ay, statesPerSample);
        _audioBufferPool = new AudioBufferPool(NumberOfBuffers, 4 * SamplesPerFrame + 16);
    }

    internal void NewFrame()
    {
        _beeperAudio.NewFrame();
        _ayAudio.NewFrame();
    }

    internal AudioBuffer EndFrame()
    {
        if (_isMuted)
        {
            return AudioBuffer.Empty;
        }

        var playAudio = false;

        if (IsAySupported && IsAyEnabled)
        {
            _ayAudio.EndFrame();
            playAudio = true;
        }

        if (IsBeeperEnabled)
        {
            _beeperAudio.EndFrame();
            playAudio = true;
        }

        if (!playAudio)
        {
            return AudioBuffer.Empty;
        }

        var audioBuffer = _audioBufferPool.GetBuffer();
        var samplesCount = _beeperAudio.Samples.Count == 0 ? Ay.ChannelA.Samples.Count : _beeperAudio.Samples.Count;

        for (var i = 0; i < samplesCount; i++)
        {
            var sample = _beeperAudio.Samples.Count > i ? _beeperAudio.Samples[i] : 0;
            int? sampleL = null;
            int? sampleR = null;

            if (IsAySupported && Ay.ChannelA.Samples.Count > i)
            {
                switch (StereoMode)
                {
                    case StereoMode.Mono:
                        sample = MonoMix(sample, Ay.ChannelA.Samples[i], Ay.ChannelB.Samples[i], Ay.ChannelC.Samples[i]);
                        break;

                    case StereoMode.StereoABC:
                        (sampleL, sampleR) = StereoMix(sample, Ay.ChannelA.Samples[i], Ay.ChannelB.Samples[i], Ay.ChannelC.Samples[i]);
                        break;

                    case StereoMode.StereoACB:
                        (sampleL, sampleR) = StereoMix(sample, Ay.ChannelA.Samples[i], Ay.ChannelC.Samples[i], Ay.ChannelB.Samples[i]);
                        break;
                }
            }

            if (StereoMode == StereoMode.Mono)
            {
                audioBuffer.Add((short)sample);
            }
            else
            {
                audioBuffer.Add((short)(sampleL ?? sample));
                audioBuffer.Add((short)(sampleR ?? sample));
            }
        }

        _audioPlayer?.TryEnqueue(audioBuffer.Buffer, audioBuffer.Count);

        return audioBuffer;
    }

    internal void Start()
    {
        if (_isAudioPlayerRunning)
        {
            return;
        }

        _audioPlayer = new AudioPlayer(
            PlayerAudioFormat,
            PlayerSampleRate,
            channelCount: StereoMode == StereoMode.Mono ? 1 : 2,
            new PlayerOptions
            {
                BufferSizeInBytes = 65536,
                BufferQueueSize = NumberOfBuffers
            });

        _audioPlayer.Volume = 100;
        _audioPlayer.Start();
        _isAudioPlayerRunning = true;
    }

    internal void Stop()
    {
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

    private void Restart()
    {
        Stop();
        Start();
    }

    private static (int, int) StereoMix(int beeperSample, int leftChannelSample, int centerChannelSample, int rightChannelSample)
    {
        var center = (int)(centerChannelSample * 0.7) + beeperSample;
        var left = leftChannelSample + center + (int)(rightChannelSample * 0.3);
        var right = (int)(leftChannelSample * 0.3) + center + rightChannelSample;

        return (left, right);
    }

    private static int MonoMix(int beeperSample, int leftChannelSample, int centerChannelSample, int rightChannelSample) =>
        leftChannelSample + centerChannelSample + rightChannelSample + beeperSample;
}
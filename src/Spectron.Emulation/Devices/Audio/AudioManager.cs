using OldBit.Beep;
using OldBit.Spectron.Emulation.Devices.Audio.AY;
using OldBit.Spectron.Emulation.Devices.Audio.Beeper;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulation.Devices.Audio;

public sealed class AudioManager
{
    private const int PlayerSampleRate = 44100;
    private const int NumberOfBuffers = 4;

    private readonly BeeperAudio _beeperAudio;

    private bool _isAyAudioEnabled;
    private AudioPlayer? _audioPlayer;
    private bool _isAudioPlayerRunning;

    internal BeeperDevice Beeper { get; }

    internal AY8910 AY { get; }

    public bool IsBeeperEnabled
    {
        get => _beeperAudio.IsEnabled;
        set
        {
            if (_beeperAudio.IsEnabled == value) return;
            ToggleBeeperEnabled(value);
        }
    }

    public bool IsAyAudioEnabled
    {
        get => _isAyAudioEnabled;
        set
        {
            if (_isAyAudioEnabled == value) return;

            _isAyAudioEnabled = value;
            AY.IsEnabled = value;
        }
    }

    public bool IsAyAudioEnabled48K { get; set; }

    internal AudioManager(Clock clock, CassettePlayer? tapePlayer, HardwareSettings hardware)
    {
        _beeperAudio = new BeeperAudio(clock, hardware, PlayerSampleRate, NumberOfBuffers);
        AY = new AY8910(clock);

        Beeper = new BeeperDevice(tapePlayer)
        {
            OnUpdateBeeper = _beeperAudio.Update
        };
    }

    internal void EndFrame(int frameTicks)
    {
        AY.EndFrame(frameTicks);

        var buffer = _beeperAudio.EndFrame();
        if (buffer != null)
        {
            _audioPlayer?.TryEnqueue(buffer.Buffer);
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

        _audioPlayer.Volume = 50;
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
    }

    public void Mute()
    {
        _beeperAudio.Mute();
    }

    public void UnMute()
    {
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
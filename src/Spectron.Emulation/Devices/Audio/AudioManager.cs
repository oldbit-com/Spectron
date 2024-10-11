using OldBit.Beep;
using OldBit.Spectron.Emulation.Devices.Audio.AY;

namespace OldBit.Spectron.Emulation.Devices.Audio;

public sealed class AudioManager
{
    private const int PlayerSampleRate = 44100;
    private const int BufferCount = 4;

    private bool _isAyAudioEnabled;
    private AudioPlayer? _audioPlayer;
    private bool _isAudioPlayerRunning;

    internal Beeper.Beeper Beeper { get; }

    internal AY8910 Ay { get; }

    public bool IsBeeperEnabled
    {
        get => Beeper.IsEnabled;
        set
        {
            if (Beeper.IsEnabled == value) return;
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
            Ay.IsEnabled = value;
        }
    }

    public bool IsAyAudioEnabled48K { get; set; }

    internal AudioManager(HardwareSettings hardware)
    {
        Beeper = new Beeper.Beeper(hardware, PlayerSampleRate, BufferCount);
        Ay = new AY8910();
    }

    internal void EndFrame(int frameTicks)
    {
        Ay.EndFrame(frameTicks);

        var buffer = Beeper.EndFrame(frameTicks);
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
                MaxQueueSize = BufferCount,
            });

        _audioPlayer.Volume = 50;
        _audioPlayer.Start();
        _isAudioPlayerRunning = true;
    }

    internal void Stop()
    {
        Beeper.Stop();

        _isAudioPlayerRunning = false;
        _audioPlayer?.Stop();

        _audioPlayer?.Dispose();
    }

    internal void ResetAudio()
    {
        Beeper.Reset();
    }

    public void Mute()
    {
        Beeper.Mute();
    }

    public void UnMute()
    {
        Beeper.UnMute();
    }

    private void ToggleBeeperEnabled(bool isEnabled)
    {
        Beeper.IsEnabled = isEnabled;

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
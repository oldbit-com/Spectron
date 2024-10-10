namespace OldBit.Spectron.Emulation.Devices.Audio;

public sealed class AudioManager
{
    private readonly Beeper _beeper;

    public bool IsBeeperEnabled { get; set; }

    public bool IsAyAudioEnabled { get; set; }

    public bool IsAyAudioEnabled48K { get; set; }

    internal AudioManager(HardwareSettings hardware)
    {
        _beeper = new Beeper(hardware);
    }

    internal void UpdateBeeper(int frameTicks, byte value)
    {
        _beeper.Update(frameTicks, value);
    }

    internal void EndFrame(int frameTicks)
    {
        _beeper.EndFrame(frameTicks);
    }

    internal void Start()
    {
        _beeper.Start();
    }

    internal void Stop()
    {
        _beeper.Stop();
    }

    internal void ResetAudio()
    {
        _beeper.Reset();
    }

    public void Mute()
    {
        _beeper.Mute();
    }

    public void UnMute()
    {
        _beeper.UnMute();
    }
}
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;

namespace OldBit.Spectron.Emulator.Tests.Devices.Audio;

public class AudioManagerTests
{
    [Fact]
    public void AudioManager_ShouldCreateCorrectly()
    {
        var clock = new EmulatorClock(Hardware.Spectrum128K.TicksPerFrame);
        var audionManager = new AudioManager(clock, null, Hardware.Spectrum128K, port => (port & 0x01) == 0);

        audionManager.IsAySupported.ShouldBeTrue();
        audionManager.StereoMode.ShouldBe(StereoMode.Mono);

        audionManager.IsBeeperEnabled.ShouldBeFalse();
        audionManager.IsAyEnabled.ShouldBeFalse();
    }
}
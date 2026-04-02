using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulator.Tests.Devices.Audio;

public class AudioManagerTests
{
    [Fact]
    public void AudioManager_ShouldCreateCorrectly()
    {
        var clock = new Clock();
        var audionManager = new AudioManager(clock, null, Hardware.Spectrum128K);

        audionManager.IsAySupported.ShouldBeTrue();
        audionManager.StereoMode.ShouldBe(StereoMode.Mono);

        audionManager.IsBeeperEnabled.ShouldBeFalse();
        audionManager.IsAyEnabled.ShouldBeFalse();
    }
}
using OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

namespace OldBit.Spectron.Emulator.Tests.Devices.Interface1;

public class MicrodriveTests
{
    [Fact]
    public void NewCartridge_ShouldShowAsInserted()
    {
        var microdrive = new Microdrive();

        microdrive.NewCartridge();

        microdrive.Cartridge.ShouldNotBeNull();
        microdrive.IsCartridgeInserted.ShouldBeTrue();
        microdrive.IsCartridgeWriteProtected.ShouldBeFalse();
    }
}
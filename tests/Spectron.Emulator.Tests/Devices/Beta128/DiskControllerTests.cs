using OldBit.Spectron.Emulation.Devices.Beta128.Controller;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128;

public class DiskControllerTests
{
    [Fact]
    public void Test()
    {
        var controller = new DiskController(3.5f);
    }
}
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Mouse;

namespace OldBit.Spectron.Emulator.Tests.Devices.Mouse;

public class MouseManagerTests
{
    [Fact]
    public void MouseManager_ShouldUpdateMouseValues()
    {
        var bus = new SpectrumBus();
        var manager = new MouseManager(bus);
        manager.Configure(MouseType.Kempston);

        manager.UpdateMouseButtons(MouseButtons.Right);
        manager.UpdatePosition(0x12, 0x34);

        var x = bus.Read(0xFBDF);
        var y = bus.Read(0xFFDF);
        var buttons = bus.Read(0xFADF);

        buttons.ShouldBe((byte)MouseButtons.Right);
        x.ShouldBe(0x12);
        y.ShouldBe(0x34);
    }
}
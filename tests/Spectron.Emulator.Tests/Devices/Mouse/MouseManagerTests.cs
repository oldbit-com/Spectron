using OldBit.Spectron.Emulation.Devices.Mouse;

namespace OldBit.Spectron.Emulator.Tests.Devices.Mouse;

public class MouseManagerTests
{
    [Fact]
    public void MouseManager_ShouldUpdateMouseValues()
    {
        var manager = new MouseManager();

        manager.UpdateMouseButtons(MouseButtons.Right);
        manager.UpdatePosition(0x12, 0x34);

        manager.Mouse.Buttons.ShouldBe(MouseButtons.Right);
        manager.Mouse.X.ShouldBe(0x12);
        manager.Mouse.Y.ShouldBe(0x34);
    }
}
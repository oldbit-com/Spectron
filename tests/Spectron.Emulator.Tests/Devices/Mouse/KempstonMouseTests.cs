using OldBit.Spectron.Emulation.Devices.Mouse;

namespace OldBit.Spectron.Emulator.Tests.Devices.Mouse;

public class KempstonMouseTests
{
    private const Word XAxis = 0xFBDF;
    private const Word YAxis = 0xFFDF;
    private const Word Buttons = 0xFADF;

    [Theory]
    [InlineData(XAxis)]
    [InlineData(YAxis)]
    [InlineData(Buttons)]
    public void WhenDisabled_PortReadShouldReturnNull(Word port)
    {
        var mouse = new KempstonMouse { IsEnabled = false };

        var value = mouse.ReadPort(port);

        value.ShouldBeNull();
    }

    [Fact]
    public void WhenReadingXAxis_ShouldReturnXValue()
    {
        var mouse = new KempstonMouse { IsEnabled = true, X = 0x12 };

        var value = mouse.ReadPort(XAxis);

        value.ShouldNotBeNull();
        value.Value.ShouldBe(0x12);
    }

    [Fact]
    public void WhenReadingYAxis_ShouldReturnYValue()
    {
        var mouse = new KempstonMouse { IsEnabled = true, Y = 0x34 };

        var value = mouse.ReadPort(YAxis);

        value.ShouldNotBeNull();
        value.Value.ShouldBe(0x34);
    }

    [Theory]
    [InlineData(MouseButtons.None)]
    [InlineData(MouseButtons.Left)]
    [InlineData(MouseButtons.Right)]
    [InlineData(MouseButtons.Middle)]
    [InlineData(MouseButtons.Left | MouseButtons.Right)]
    [InlineData(MouseButtons.Left | MouseButtons.Right | MouseButtons.Middle)]
    public void WhenReadingButtons_ShouldReturnButtonValue(MouseButtons buttons)
    {
        var mouse = new KempstonMouse { IsEnabled = true, Buttons = buttons };

        var value = mouse.ReadPort(Buttons);

        value.ShouldBe((byte?)buttons);
    }
}
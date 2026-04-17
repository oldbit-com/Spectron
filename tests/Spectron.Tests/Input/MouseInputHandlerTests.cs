using Avalonia;
using Avalonia.Input;
using OldBit.Spectron.Emulation.Devices;
using OldBit.Spectron.Emulation.Devices.Mouse;
using OldBit.Spectron.Input;
using OldBit.Spectron.Screen;

namespace OldBit.Spectron.Tests.Input;

public class MouseInputHandlerTests
{
    private const Word XAxisPort = 0xFBDF;
    private const Word YAxisPort = 0xFFDF;
    private const Word ButtonsPort = 0xFADF;

    private readonly MouseManager _mouseManager;
    private readonly MouseInputHandler _mouseInputHandler;
    private readonly SpectrumBus _spectrumBus;

    public MouseInputHandlerTests()
    {
        _spectrumBus = new SpectrumBus();
        _mouseManager = new MouseManager(_spectrumBus);
        _mouseInputHandler = new MouseInputHandler(_mouseManager);
    }

    [Fact]
    public void WithNoMouse_ReadsAllPortsAsFF()
    {
        _mouseManager.Configure(MouseType.None);

        var x = _spectrumBus.Read(XAxisPort);
        var y = _spectrumBus.Read(YAxisPort);
        var buttons = _spectrumBus.Read(ButtonsPort);

        x.ShouldBe((byte)0xFF);
        y.ShouldBe((byte)0xFF);
        buttons.ShouldBe((byte)0xFF);
    }

    [Fact]
    public void WithKempstonMouse_DefaultState_ReadsAllPortsAs00()
    {
        _mouseManager.Configure(MouseType.Kempston);

        var x = _spectrumBus.Read(XAxisPort);
        var y = _spectrumBus.Read(YAxisPort);
        var buttons = _spectrumBus.Read(ButtonsPort);

        x.ShouldBe((byte)0);
        y.ShouldBe((byte)0);
        buttons.ShouldBe((byte)MouseButtons.None);
    }

    [Theory]
    [InlineData(21.551906779661017, 20.702330508474578, 0, 255)]
    [InlineData(110.91207627118644, 5.8559322033898304, 106, 255)]
    [InlineData(69.1260593220339, 1.930084745762712, 57, 255)]
    [InlineData(255.85911016949154, 79.6302966101695, 255, 156)]
    [InlineData(234.1949152542373, 21.829449152542374, 254, 254)]
    [InlineData(234.90482076637824, 171.69066749072928, 254, 2)]
    [InlineData(117.41737288135593, 179.20338983050848, 114, 0)]
    [InlineData(22.93201483312732, 171.2762669962917, 2, 2)]
    [InlineData(7.988347457627119, 84.5, 0, 148)]
    public void WithKempstonMouse_MouseMoved_UpdatesPorts(double posX, double posY, byte expectedX, byte expectedY)
    {
        _mouseManager.Configure(MouseType.Kempston);

        var bounds = new Rect(0, 0, 256, 192);
        var position = new Point(posX, posY);

        _mouseInputHandler.MouseMoved(BorderSize.Medium, position, bounds);

        var x = _spectrumBus.Read(XAxisPort);
        var y = _spectrumBus.Read(YAxisPort);
        var buttons = _spectrumBus.Read(ButtonsPort);

        x.ShouldBe(expectedX);
        y.ShouldBe(expectedY);
        buttons.ShouldBe((byte)MouseButtons.None);
    }

    [Theory]
    [InlineData(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed, MouseButtons.Left)]
    [InlineData(RawInputModifiers.RightMouseButton, PointerUpdateKind.RightButtonPressed, MouseButtons.Right)]
    [InlineData(RawInputModifiers.MiddleMouseButton, PointerUpdateKind.MiddleButtonPressed, MouseButtons.Middle)]
    public void WithKempstonMouse_ButtonPressed_UpdatesPorts(RawInputModifiers modifiers, PointerUpdateKind kind, MouseButtons expectedButtons)
    {
        _mouseManager.Configure(MouseType.Kempston);

        _mouseInputHandler.ButtonsStateChanged(new PointerPoint(
            new Pointer(0, PointerType.Mouse, true),
            new Point(),
            new PointerPointProperties(modifiers, kind)));

        var buttons = _spectrumBus.Read(ButtonsPort);

        buttons.ShouldBe((byte)expectedButtons);
    }
}
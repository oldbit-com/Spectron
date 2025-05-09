namespace OldBit.Spectron.Emulation.Devices.Mouse;

internal interface IMouse : IDevice
{
    byte X { set; }

    byte Y { set; }

    MouseButtons Buttons { set; }
}
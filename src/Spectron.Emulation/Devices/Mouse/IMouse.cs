namespace OldBit.Spectron.Emulation.Devices.Mouse;

internal interface IMouse : IDevice
{
    byte X { get; set; }

    byte Y { get; set; }

    MouseButtons Buttons { get; set; }
}
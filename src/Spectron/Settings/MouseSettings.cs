using OldBit.Spectron.Emulation.Devices.Mouse;

namespace OldBit.Spectron.Settings;

public record MouseSettings
{
    public MouseType MouseType { get; init; }

    public bool IsStandardMousePointerHidden { get; init; } = true;
}
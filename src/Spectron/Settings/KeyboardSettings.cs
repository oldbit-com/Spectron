using SharpHook.Data;

namespace OldBit.Spectron.Settings;

public record KeyboardSettings
{
    public KeyCode CapsShiftKey { get; init; } = KeyCode.VcLeftShift;
    public KeyCode SymbolShiftKey { get; init; } = KeyCode.VcRightAlt;
}
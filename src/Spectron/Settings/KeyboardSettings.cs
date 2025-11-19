using Avalonia.Input;

namespace OldBit.Spectron.Settings;

public record KeyboardSettings
{
    public Key CapsShiftKey { get; init; } = Key.LeftShift;

    public Key SymbolShiftKey { get; init; } = Key.RightAlt;

    public bool ShouldHandleExtendedKeys  { get; init; } = true;
}
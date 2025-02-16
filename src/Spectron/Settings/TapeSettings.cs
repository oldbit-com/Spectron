using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.Settings;

public record TapeSettings
{
    public bool IsAutoPlayEnabled { get; init; } = true;

    public bool IsSaveEnabled { get; init; } = true;

    public TapeSpeed SaveSpeed { get; init; } = TapeSpeed.Normal;

    public TapeSpeed LoadSpeed { get; init; } = TapeSpeed.Normal;
}
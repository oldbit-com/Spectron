using OldBit.Spectron.Database;

namespace OldBit.Spectron.Settings;

public class GameBrowserSettings
{
    public string? ZxDbPath { get; init; }

    public GameSearchField SearchField { get; init; } = GameSearchField.Title;
}

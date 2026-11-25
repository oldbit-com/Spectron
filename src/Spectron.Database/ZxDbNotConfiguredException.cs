namespace OldBit.Spectron.Database;

public sealed class ZxDbNotConfiguredException(string? databasePath)
    : InvalidOperationException(string.IsNullOrWhiteSpace(databasePath)
        ? "ZXDB database location has not been set."
        : $"ZXDB database file '{databasePath}' does not exist.");

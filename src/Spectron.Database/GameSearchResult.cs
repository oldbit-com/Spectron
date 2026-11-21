namespace OldBit.Spectron.Database;

public sealed record GameSearchResult(
    int Id,
    string Title,
    string? Machine,
    string? Genre,
    short? Year,
    string? Availability,
    string? Publisher,
    string? Screenshot);

namespace OldBit.Spectron.Database.Results;

public sealed record GameSearchResult(
    int Id,
    string Title,
    string? Machine,
    string? Genre,
    short? Year,
    string? Availability,
    string? AvailabilityType,
    string? Publisher);

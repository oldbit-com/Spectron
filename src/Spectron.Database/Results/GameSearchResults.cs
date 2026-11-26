namespace OldBit.Spectron.Database.Results;

public sealed record GameSearchResults(IReadOnlyList<GameSearchResult> Games, bool IsTruncated);

namespace OldBit.Spectron.Database.Results;

/// <summary>
/// Games matching a search, up to the search limit.
/// </summary>
/// <param name="Games">The matching games.</param>
/// <param name="IsTruncated">True when more games matched than the limit allowed.</param>
public sealed record GameSearchResults(IReadOnlyList<GameSearchResult> Games, bool IsTruncated);

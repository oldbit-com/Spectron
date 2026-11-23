using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OldBit.Spectron.Database.Data;

namespace OldBit.Spectron.Database;

public class ZxDbClient(IDbContextFactory<GamesDbContext> contextFactory, ZxDbSettings settings)
{
    private const byte ScreenshotFileTypeId = 1;

    public bool IsAvailable => settings.IsAvailable;

    public static async Task<bool> IsValidDatabaseAsync(string databasePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(databasePath))
        {
            return false;
        }

        try
        {
            await using var connection = new SqliteConnection(ZxDbContextFactory.CreateConnectionString(databasePath));
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name IN ('entries', 'search_by_titles')";

            var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

            return count == 2;
        }
        catch (SqliteException)
        {
            return false;
        }
    }

    public async Task<List<GameSearchResult>> SearchByTitleAsync(
        string title,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        var searchTitle = NormalizeSearchTitle(title);

        if (searchTitle.Length == 0)
        {
            return [];
        }

        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        var pattern = $"%{searchTitle}%";

        return await db.Entries
            .Where(e => db.SearchByTitles.Any(s => s.EntryId == e.Id && EF.Functions.Like(s.EntryTitle, pattern)))
            .OrderBy(e => e.Title)
            .Select(e => new GameSearchResult(
                e.Id,
                e.Title,
                e.Machinetype!.Text,
                e.Genretype!.Text,
                e.Releases
                    .Where(r => r.ReleaseSeq == 0)
                    .Select(r => r.ReleaseYear)
                    .FirstOrDefault(),
                e.Availabletype!.Text,
                e.Publishers
                    .Where(p => p.ReleaseSeq == 0 && p.PublisherSeq == 1)
                    .Select(p => p.Label.Name)
                    .FirstOrDefault(),
                e.Downloads
                    .Where(d => d.FiletypeId == ScreenshotFileTypeId)
                    .OrderBy(d => d.Id)
                    .Select(d => d.FileLink)
                    .FirstOrDefault()))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    // search_by_titles stores titles lower-cased with everything except letters and digits removed,
    // e.g. "Jet Set Willy 64: Manic Miner" -> "jetsetwilly64manicminer".
    private static string NormalizeSearchTitle(string title) =>
        string.Concat(title.Where(char.IsLetterOrDigit)).ToLowerInvariant();
}

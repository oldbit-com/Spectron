using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OldBit.Spectron.Database.Data;
using OldBit.Spectron.Database.Results;

namespace OldBit.Spectron.Database;

public sealed class ZxDbClient(IDbContextFactory<GamesDbContext> contextFactory, ZxDbSettings settings)
{
    private const byte ScreenshotFileTypeId = 1;
    private const byte TapeImageFileTypeId = 8;
    private const byte SnapshotFileTypeId = 10;
    private const byte BugFixTapeImageFileTypeId = 22;
    private const byte RzxFileTypeId = 63;
    private const byte PokFileTypeId = 74;

    // Machine types 1-11 cover the ZX Spectrum 16K up to the Timex TC2048, 13 is TC2048/Tx2068.
    // Entries without a machine type (magazines, covertapes, compilations) are Spectrum software too.
    private const byte LastSpectrumMachineTypeId = 11;
    private const byte TimexTc2048Tx2068MachineTypeId = 13;

    private const string ScrFilePattern = "%.scr";

    internal bool IsAvailable => settings.IsAvailable;

    internal static async Task<bool> IsValidDatabaseAsync(string databasePath, CancellationToken cancellationToken = default)
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

    internal async Task<List<GameSearchResult>> SearchByTitleAsync(
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
        var titles = db.SearchByTitles;

        return await db.Entries
            .Where(e => titles.Any(s => s.EntryId == e.Id && EF.Functions.Like(s.EntryTitle, pattern)))
            .Where(e => e.MachinetypeId == null ||
                        e.MachinetypeId <= LastSpectrumMachineTypeId ||
                        e.MachinetypeId == TimexTc2048Tx2068MachineTypeId)
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
                    .FirstOrDefault()))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    internal async Task<string?> GetScreenshotFileAsync(int entryId, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Only raw .scr screens can be rendered, some loading screens are stored as gif, png, etc.
        return await db.Downloads
            .Where(e => e.EntryId == entryId &&
                        e.FiletypeId == ScreenshotFileTypeId &&
                        EF.Functions.Like(e.FileLink, ScrFilePattern))
            .OrderBy(e => e.Id)
            .Select(e => e.FileLink)
            .FirstOrDefaultAsync(cancellationToken);
    }

    internal async Task<List<FileResult>> GetGameFileLinksAsync(int entryId, CancellationToken cancellationToken = default)
    {
        await using var db = await contextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Downloads
            .Where(e => e.EntryId == entryId &&
                        (e.FiletypeId == TapeImageFileTypeId ||
                         e.FiletypeId == SnapshotFileTypeId ||
                         e.FiletypeId == BugFixTapeImageFileTypeId ||
                         e.FiletypeId == RzxFileTypeId ||
                         e.FiletypeId == PokFileTypeId))
            .OrderBy(e => e.ReleaseSeq)
            .ThenBy(e => e.Id)
            .Select(e => new FileResult(e.FileLink))
            .ToListAsync(cancellationToken);
    }

    // search_by_titles stores titles lower-cased with everything except letters and digits removed,
    // e.g. "Jet Set Willy 64: Manic Miner" -> "jetsetwilly64manicminer".
    private static string NormalizeSearchTitle(string title) =>
        string.Concat(title.Where(char.IsLetterOrDigit)).ToLowerInvariant();
}

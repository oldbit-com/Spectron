using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace OldBit.Spectron.Database.Data;

internal sealed class ZxDbContextFactory(ZxDbSettings settings) : IDbContextFactory<GamesDbContext>
{
    private readonly Lock _lock = new();

    private string? _cachedPath;
    private DbContextOptions<GamesDbContext>? _cachedOptions;

    public GamesDbContext CreateDbContext()
    {
        var databasePath = settings.DatabasePath;

        if (!settings.IsAvailable)
        {
            throw new ZxDbNotConfiguredException(databasePath);
        }

        return new GamesDbContext(GetOptions(databasePath!));
    }

    private DbContextOptions<GamesDbContext> GetOptions(string databasePath)
    {
        lock (_lock)
        {
            if (_cachedOptions != null && _cachedPath == databasePath)
            {
                return _cachedOptions;
            }

            _cachedOptions = new DbContextOptionsBuilder<GamesDbContext>()
                .UseSqlite(CreateConnectionString(databasePath))
                .Options;

            _cachedPath = databasePath;

            return _cachedOptions;
        }
    }

    internal static string CreateConnectionString(string databasePath) => new SqliteConnectionStringBuilder
    {
        DataSource = databasePath,
        Mode = SqliteOpenMode.ReadOnly,
    }.ToString();
}

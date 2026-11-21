using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Database.Data;

namespace OldBit.Spectron.Database.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddZxDb(this IServiceCollection services, string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly,
        }.ToString();

        services.AddDbContextFactory<GamesDbContext>(options => options.UseSqlite(connectionString));
        services.AddSingleton<ZxDbClient>();
    }
}

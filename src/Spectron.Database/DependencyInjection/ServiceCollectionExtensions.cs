using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Database.Data;

namespace OldBit.Spectron.Database.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddZxDb(this IServiceCollection services)
    {
        services.AddSingleton<ZxDbSettings>();
        services.AddSingleton<IDbContextFactory<GamesDbContext>, ZxDbContextFactory>();
        services.AddSingleton<ZxDbClient>();
        services.AddSingleton<ZxDbService>();
        services.AddMemoryCache();
        services.AddHttpClient();
    }
}

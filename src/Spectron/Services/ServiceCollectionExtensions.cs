using Microsoft.Extensions.DependencyInjection;

namespace OldBit.Spectron.Services;

public static class ServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<ApplicationDataService>();
        services.AddSingleton<PreferencesService>();
        services.AddSingleton<SessionService>();
        services.AddSingleton<RecentFilesService>();
    }
}
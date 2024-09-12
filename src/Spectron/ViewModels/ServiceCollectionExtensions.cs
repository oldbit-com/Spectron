using Microsoft.Extensions.DependencyInjection;

namespace OldBit.Spectron.ViewModels;

public static class ServiceCollectionExtensions
{
    public static void AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<RecentFilesViewModel>();
        services.AddSingleton<TimeMachineViewModel>();
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace OldBit.Spectron.Debugger.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddDebugging(this IServiceCollection services)
    {
        services.AddSingleton<DebuggerContext>();
    }
}
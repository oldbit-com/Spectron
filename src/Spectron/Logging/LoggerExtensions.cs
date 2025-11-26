using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace OldBit.Spectron.Logging;

public static class MemoryLoggerExtensions
{
    public static ILoggingBuilder AddInMemory(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILogStore, InMemoryLogStore>();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, InMemoryLoggerProvider>());

        return builder;
    }
}
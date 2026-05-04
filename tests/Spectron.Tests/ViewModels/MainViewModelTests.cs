using Microsoft.Extensions.DependencyInjection;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Logging;
using OldBit.Spectron.Services;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class MainViewModelTests
{
    [Fact]
    public void ViewModel_ShouldInitialize()
    {
        var services = new ServiceCollection();
        services.AddServices();
        services.AddViewModels();
        services.AddEmulation();
        services.AddDebugging();
        services.AddLogging();

        services.AddSingleton<ILogStore, InMemoryLogStore>();

        var provider = services.BuildServiceProvider();

        var viewModel = provider.GetRequiredService<MainViewModel>();

        viewModel.ShouldNotBeNull();
    }
}
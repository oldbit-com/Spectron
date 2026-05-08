using Avalonia.Headless.XUnit;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class MainViewModelEmulatorTests : IDisposable
{
    private readonly MainViewModel _viewModel;

    public MainViewModelEmulatorTests()
    {
        var builder = new MainViewModelBuilder();

        _viewModel = builder.Build();

        EnsureEmulatorIsCreated();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _viewModel.Emulator?.Shutdown();
    }

    [AvaloniaFact]
    public void ShouldToggleUlaPlus()
    {
        _viewModel.ToggleUlaPlusCommand.Execute(null);

        _viewModel.IsUlaPlusEnabled.ShouldBeTrue();
        _viewModel.StatusBarViewModel.IsUlaPlusEnabled.ShouldBeTrue();
        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.UlaPlus.IsEnabled.ShouldBeTrue();

        _viewModel.ToggleUlaPlusCommand.Execute(null);

        _viewModel.IsUlaPlusEnabled.ShouldBeFalse();
        _viewModel.StatusBarViewModel.IsUlaPlusEnabled.ShouldBeFalse();
        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.UlaPlus.IsEnabled.ShouldBeFalse();
    }

    private void EnsureEmulatorIsCreated() => _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum48K);
}
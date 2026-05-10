using Avalonia.Headless.XUnit;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class MainViewModelEmulatorTests : IDisposable
{
    private readonly MainViewModel _viewModel;

    public MainViewModelEmulatorTests(ITestOutputHelper output)
    {
        var serviceProvider = new TestServiceProvider(output);
        var builder = new MainViewModelBuilder(serviceProvider);

        _viewModel = builder.Build();

        EnsureEmulatorIsCreated();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _viewModel.Dispose();
    }

    [AvaloniaFact]
    public void ShouldToggleUlaPlus()
    {
        _viewModel.ToggleUlaPlusCommand.Execute(null);

        _viewModel.IsUlaPlusEnabled.ShouldBeTrue();
        _viewModel.StatusBarViewModel.IsUlaPlusEnabled.ShouldBeTrue();
        _viewModel.Emulator?.UlaPlus.IsEnabled.ShouldBeTrue();

        _viewModel.ToggleUlaPlusCommand.Execute(null);

        _viewModel.IsUlaPlusEnabled.ShouldBeFalse();
        _viewModel.StatusBarViewModel.IsUlaPlusEnabled.ShouldBeFalse();
        _viewModel.Emulator?.UlaPlus.IsEnabled.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void ShouldTogglePause()
    {
        _viewModel.TogglePauseCommand.Execute(null);

        _viewModel.IsPaused.ShouldBeTrue();
        _viewModel.IsPauseOverlayVisible.ShouldBeTrue();
        _viewModel.Emulator?.IsPaused.ShouldBeTrue();

        _viewModel.TogglePauseCommand.Execute(null);

        _viewModel.IsPaused.ShouldBeFalse();
        _viewModel.IsPauseOverlayVisible.ShouldBeFalse();
        _viewModel.Emulator?.IsPaused.ShouldBeFalse();
    }

    [AvaloniaTheory]
    [InlineData(TapeSpeed.Normal)]
    [InlineData(TapeSpeed.Accelerated)]
    [InlineData(TapeSpeed.Instant)]
    public void ShouldChangeTapeLoadSpeed(TapeSpeed tapeSpeed)
    {
        _viewModel.SetTapeLoadSpeedCommand.Execute(tapeSpeed);

        _viewModel.Emulator?.TapeManager.TapeLoadSpeed.ShouldBe(tapeSpeed);
    }

    private void EnsureEmulatorIsCreated() => _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum48K);
}
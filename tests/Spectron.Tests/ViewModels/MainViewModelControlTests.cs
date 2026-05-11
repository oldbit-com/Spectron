using System.Diagnostics.CodeAnalysis;
using Avalonia.Headless.XUnit;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class MainViewModelControlTests(ITestOutputHelper output) : IDisposable
{
    private readonly MainViewModelBuilder _mainViewModelBuilder = new(new TestServiceProvider(output));
    private MainViewModel? _viewModel;

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _viewModel?.Dispose();
    }

    [AvaloniaFact]
    public void ShouldTogglePause()
    {
        BuildViewModel();

        _viewModel.TogglePauseCommand.Execute(null);

        _viewModel.IsPaused.ShouldBeTrue();
        _viewModel.IsPauseOverlayVisible.ShouldBeTrue();
        _viewModel.Emulator?.IsPaused.ShouldBeTrue();

        _viewModel.TogglePauseCommand.Execute(null);

        _viewModel.IsPaused.ShouldBeFalse();
        _viewModel.IsPauseOverlayVisible.ShouldBeFalse();
        _viewModel.Emulator?.IsPaused.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Reset_ShouldNotCreateNewEmulator()
    {
        _mainViewModelBuilder
            .WithFile("test.spectron")
            .WithOpenFilePicker()
            .WithStateSnapshotStore();

        BuildViewModel();

        var emulator = _viewModel.Emulator;

        _viewModel.ResetCommand.Execute(null);

        _viewModel.Emulator.ShouldBeSameAs(emulator);
        _viewModel.ComputerType.ShouldBe(ComputerType.Timex2048);
        _viewModel.RomType.ShouldBe(RomType.Original);
    }

    [AvaloniaFact]
    public void HardResetShouldCreateNewEmulator()
    {
        _mainViewModelBuilder
            .WithFile("test.spectron")
            .WithOpenFilePicker()
            .WithStateSnapshotStore();

        BuildViewModel();

        var emulator = _viewModel.Emulator;

        _viewModel.HardResetCommand.Execute(null);

        _viewModel.Emulator.ShouldNotBeSameAs(emulator);
        _viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K); // Hard reset should reset to preferred computer type
        _viewModel.RomType.ShouldBe(RomType.Original);
    }

    [AvaloniaFact]
    public void TestPreferencesView()
    {
        BuildViewModel();

        //_viewModel.ShowPreferencesViewCommand.Execute(null);
    }

    [MemberNotNull(nameof(_viewModel))]
    private void BuildViewModel()
    {
        _viewModel = _mainViewModelBuilder.Build();
        _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Timex2048);
    }
}
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.ViewModels;
using OldBit.Spectron.Views;

namespace OldBit.Spectron.Tests.Views;

public class MainWindowTests : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly MainViewModel _viewModel;

    public MainWindowTests()
    {
        var servicesProvider = new TestServiceProvider();

        AudioManager.UseSilentAudioPlayer = true;

        _viewModel = new MainViewModelBuilder(servicesProvider)
            .WithFile("test.sna")
            .WithOpenFilePicker()
            .Build();

        _mainWindow = new MainWindow(servicesProvider.Build())
        {
            DataContext = _viewModel
        };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _viewModel.Emulator?.Shutdown();
    }

    [AvaloniaFact]
    public void ShouldExecuteLoadCommand()
    {
        var loadMenuItem = _mainWindow.FindMenuItem("Load...");

        loadMenuItem.ShouldNotBeNull();
        loadMenuItem.Command.ShouldBeOfType<AsyncRelayCommand>();

        loadMenuItem.Command.Execute(null);

        _viewModel.Title.ShouldBe("Spectron - ZX Spectrum Emulator [test.sna]");
        _viewModel.Emulator.ShouldNotBeNull();

        MachineTypeShouldBeChecked("ZX Spectrum 48");
    }

    [AvaloniaFact]
    public void ShouldExecuteSaveCommand()
    {
        var saveMenuItem = _mainWindow.FindMenuItem("Save Snapshot...");

        saveMenuItem.ShouldNotBeNull();
        saveMenuItem.Command.ShouldBeOfType<AsyncRelayCommand>();

        saveMenuItem.Command.Execute(null);
    }

    [AvaloniaTheory]
    [InlineData("ZX Spectrum 16", ComputerType.Spectrum16K)]
    [InlineData("ZX Spectrum 48", ComputerType.Spectrum48K)]
    [InlineData("ZX Spectrum 128", ComputerType.Spectrum128K)]
    [InlineData("Timex Computer 2048", ComputerType.Timex2048)]
    public void ShouldChangeComputerType(string computerName, ComputerType computerType)
    {
        _mainWindow.FindMenuItem(computerName)?.Command?.Execute(computerType);
        _mainWindow.FindMenuItem(computerName)?.IsChecked.ShouldBeTrue();

        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.ComputerType.ShouldBe(computerType);

        _viewModel.StatusBarViewModel.ComputerType.ShouldBe(computerType);

        MachineTypeShouldBeChecked(computerName);
    }

    private void MachineTypeShouldBeChecked(string name)
    {
        var machineMenuItem = _mainWindow.FindMenuItem("Machine");

        machineMenuItem.ShouldNotBeNull();

        machineMenuItem.Items.OfType<MenuItem>()
            .Single(item => Equals(item.Header, name)).IsChecked.ShouldBeTrue();

        machineMenuItem.Items.OfType<MenuItem>()
            .Where(item => !Equals(item.Header, name)).All(item => item.IsChecked).ShouldBeFalse();
    }
}
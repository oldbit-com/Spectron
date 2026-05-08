using Avalonia.Headless.XUnit;
using CommunityToolkit.Mvvm.Input;
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
        _mainWindow.MainMenu.ShouldNotBeNull();

        var loadMenuItem = _mainWindow.FindMenuItem("Load...");

        loadMenuItem.ShouldNotBeNull();
        loadMenuItem.Command.ShouldBeOfType<AsyncRelayCommand>();

        ((AsyncRelayCommand)loadMenuItem.Command!).Execute(null);

        _viewModel.Title.ShouldBe("Spectron - ZX Spectrum Emulator [test.sna]");
        _viewModel.Emulator.ShouldNotBeNull();
    }
}
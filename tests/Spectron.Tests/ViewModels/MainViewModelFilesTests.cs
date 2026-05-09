using Avalonia.Headless.XUnit;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Files.Sna;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.Files.Z80;
using OldBit.Spectron.Files.Z80.Types;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class MainViewModelFilesTests(ITestOutputHelper output) : IDisposable
{
    private readonly MainViewModelBuilder _mainViewModelBuilder = new(new TestServiceProvider(output));
    private MainViewModel? _viewModel;

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _viewModel?.Emulator?.Shutdown();
    }

    [AvaloniaFact]
    public async Task ShouldSave_SnaSnapshotFile()
    {
        var snapshotSaved = new TaskCompletionSource<SnaFile>();

        var builder = _mainViewModelBuilder
            .WithFile("test.sna")
            .WithSaveFilePicker()
            .WithSnaSnapshotStore(snapshotSaved.SetResult);

        _viewModel = builder.Build();
        _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await _viewModel.SaveFileCommand.ExecuteAsync(null);
        var snapshot = await snapshotSaved.Task.WaitAsync(TimeSpan.FromSeconds(1));

        snapshot.Header128.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task ShouldSave_SzxSnapshotFile()
    {
        var snapshotSaved = new TaskCompletionSource<SzxFile>();

        var builder = _mainViewModelBuilder
            .WithFile("test.szx")
            .WithSaveFilePicker()
            .WithSzxSnapshotStore(snapshotSaved.SetResult);

        _viewModel = builder.Build();
        _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await _viewModel.SaveFileCommand.ExecuteAsync(null);
        var snapshot = await snapshotSaved.Task.WaitAsync(TimeSpan.FromSeconds(1));

        snapshot.Header.MachineId.ShouldBe(SzxHeader.MachineId128K);
    }

    [AvaloniaFact]
    public async Task ShouldSave_Z80SnapshotFile()
    {
        var snapshotSaved = new TaskCompletionSource<Z80File>();

        var builder = _mainViewModelBuilder
            .WithFile("test.z80")
            .WithSaveFilePicker()
            .WithZ80SnapshotStore(snapshotSaved.SetResult);

        _viewModel = builder.Build();
        _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await _viewModel.SaveFileCommand.ExecuteAsync(null);
        var snapshot = await snapshotSaved.Task.WaitAsync(TimeSpan.FromSeconds(1));

        snapshot.Header.HardwareMode.ShouldBe(HardwareMode.Spectrum128);
    }

    [AvaloniaFact]
    public async Task ShouldSave_SpectronSnapshotFile()
    {
        var snapshotSaved = new TaskCompletionSource<StateSnapshot>();

        var builder = _mainViewModelBuilder
            .WithFile("test.spectron")
            .WithSaveFilePicker()
            .WithStateSnapshotStore(snapshotSaved.SetResult);

        _viewModel = builder.Build();
        _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await _viewModel.SaveFileCommand.ExecuteAsync(null);
        var snapshot = await snapshotSaved.Task.WaitAsync(TimeSpan.FromSeconds(1));

        snapshot.ComputerType.ShouldBe(ComputerType.Spectrum128K);
    }

    [AvaloniaFact]
    public async Task ShouldShowError_UnknownFileType()
    {
        string? errorMessage = null;

        var builder = _mainViewModelBuilder
            .WithFile("file.unknown")
            .WithMessageDialogs(message => errorMessage = message)
            .WithSaveFilePicker();

        _viewModel = builder.Build();
        _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await _viewModel.SaveFileCommand.ExecuteAsync(null);

        errorMessage.ShouldNotBeNull();
        errorMessage.ShouldBe("The file extension '.unknown' is not supported.");
    }

    [AvaloniaFact]
    public async Task ShouldLoad_StateSnapshotFile()
    {
        var builder = _mainViewModelBuilder
            .WithFile("test.spectron")
            .WithOpenFilePicker()
            .WithStateSnapshotStore();

        _viewModel = builder.Build();

        _viewModel.Emulator.ShouldBeNull();

        await _viewModel.LoadFileCommand.ExecuteAsync(null);

        _viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }

    [AvaloniaFact]
    public async Task ShouldLoad_SnaSnapshotFile()
    {
        var builder = _mainViewModelBuilder
            .WithFile("test.sna")
            .WithOpenFilePicker()
            .WithSnaSnapshotStore();

        _viewModel = builder.Build();

        _viewModel.Emulator.ShouldBeNull();

        await _viewModel.LoadFileCommand.ExecuteAsync(null);

        _viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }

    [AvaloniaFact]
    public async Task ShouldLoad_Z80SnapshotFile()
    {
        var builder = _mainViewModelBuilder
            .WithFile("test.z80")
            .WithOpenFilePicker()
            .WithZ80SnapshotStore();

        _viewModel = builder.Build();

        _viewModel.Emulator.ShouldBeNull();

        await _viewModel.LoadFileCommand.ExecuteAsync(null);

        _viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }

    [AvaloniaFact]
    public async Task ShouldLoad_SzxSnapshotFile()
    {
        var builder = _mainViewModelBuilder
            .WithFile("test.szx")
            .WithOpenFilePicker()
            .WithSzxSnapshotStore();

        _viewModel = builder.Build();

        _viewModel.Emulator.ShouldBeNull();

        await _viewModel.LoadFileCommand.ExecuteAsync(null);

        _viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }

    [AvaloniaFact]
    public async Task ShouldQuickSave()
    {
        var snapshotSaved = new TaskCompletionSource<StateSnapshot>();

        var builder = _mainViewModelBuilder
            .WithFile("test.spectron")
            .WithStateSnapshotStore(snapshotSaved.SetResult, matchAnyName: true);

        _viewModel = builder.Build();
        _viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        _viewModel.QuickSaveCommand.Execute(null);

        var snapshot = await snapshotSaved.Task.WaitAsync(TimeSpan.FromSeconds(1));

        snapshot.ShouldNotBeNull();
        snapshot.ComputerType.ShouldBe(ComputerType.Spectrum128K);
    }

    [AvaloniaFact]
    public void ShouldQuickLoad()
    {
        var builder = _mainViewModelBuilder
            .WithFile("quick-save.spectron")
            .WithStateSnapshotStore();

        _viewModel = builder.Build();

        _viewModel.Emulator.ShouldBeNull();

        _viewModel.QuickLoadCommand.Execute(null);

        _viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        _viewModel.Emulator.ShouldNotBeNull();
        _viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }
}
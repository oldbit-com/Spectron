using Avalonia.Headless.XUnit;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Files.Sna;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.Files.Z80;
using OldBit.Spectron.Files.Z80.Types;

namespace OldBit.Spectron.Tests.ViewModels;

public class MainViewModelFilesTests
{
    private readonly MainViewModelBuilder _mainViewModelBuilder = new(new TestServiceProvider());

    [AvaloniaFact]
    public async Task ShouldSave_SnaSnapshotFile()
    {
        var snapshotSaved = new TaskCompletionSource<SnaFile>();

        var builder = _mainViewModelBuilder
            .WithFile("test.sna")
            .WithSaveFilePicker()
            .WithSnaSnapshotStore(snapshotSaved.SetResult);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);
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

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);
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

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);
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

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);
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

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);

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

        var viewModel = builder.Build();

        viewModel.Emulator.ShouldBeNull();

        await viewModel.LoadFileCommand.ExecuteAsync(null);

        viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        viewModel.Emulator.ShouldNotBeNull();
        viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }

    [AvaloniaFact]
    public async Task ShouldLoad_SnaSnapshotFile()
    {
        var builder = _mainViewModelBuilder
            .WithFile("test.sna")
            .WithOpenFilePicker()
            .WithSnaSnapshotStore();

        var viewModel = builder.Build();

        viewModel.Emulator.ShouldBeNull();

        await viewModel.LoadFileCommand.ExecuteAsync(null);

        viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        viewModel.Emulator.ShouldNotBeNull();
        viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }

    [AvaloniaFact]
    public async Task ShouldLoad_Z80SnapshotFile()
    {
        var builder = _mainViewModelBuilder
            .WithFile("test.z80")
            .WithOpenFilePicker()
            .WithZ80SnapshotStore();

        var viewModel = builder.Build();

        viewModel.Emulator.ShouldBeNull();

        await viewModel.LoadFileCommand.ExecuteAsync(null);

        viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        viewModel.Emulator.ShouldNotBeNull();
        viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }

    [AvaloniaFact]
    public async Task ShouldLoad_SzxSnapshotFile()
    {
        var builder = _mainViewModelBuilder
            .WithFile("test.szx")
            .WithOpenFilePicker()
            .WithSzxSnapshotStore();

        var viewModel = builder.Build();

        viewModel.Emulator.ShouldBeNull();

        await viewModel.LoadFileCommand.ExecuteAsync(null);

        viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        viewModel.Emulator.ShouldNotBeNull();
        viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }

    [AvaloniaFact]
    public async Task ShouldQuickSave()
    {
        var snapshotSaved = new TaskCompletionSource<StateSnapshot>();

        var builder = _mainViewModelBuilder
            .WithFile("test.spectron")
            .WithStateSnapshotStore(snapshotSaved.SetResult, matchAnyName: true);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        viewModel.QuickSaveCommand.Execute(null);

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

        var viewModel = builder.Build();

        viewModel.Emulator.ShouldBeNull();

        viewModel.QuickLoadCommand.Execute(null);

        viewModel.ComputerType.ShouldBe(ComputerType.Spectrum48K);
        viewModel.Emulator.ShouldNotBeNull();
        viewModel.Emulator.ComputerType.ShouldBe(ComputerType.Spectrum48K);
    }
}
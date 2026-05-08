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
        SnaFile? snaSnapshot = null;

        var builder = _mainViewModelBuilder
            .WithFile("test.sna")
            .WithSaveFilePicker()
            .WithSnaSnapshotStore(snapshot => snaSnapshot = snapshot);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);

        snaSnapshot.ShouldNotBeNull();
        snaSnapshot.Header128.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task ShouldSave_SzxSnapshotFile()
    {
        SzxFile? szxSnapshot = null;

        var builder = _mainViewModelBuilder
            .WithFile("test.szx")
            .WithSaveFilePicker()
            .WithSzxSnapshotStore(snapshot => szxSnapshot = snapshot);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);

        szxSnapshot.ShouldNotBeNull();
        szxSnapshot.Header.MachineId.ShouldBe(SzxHeader.MachineId128K);
    }

    [AvaloniaFact]
    public async Task ShouldSave_Z80SnapshotFile()
    {
        Z80File? z80Snapshot = null;

        var builder = _mainViewModelBuilder
            .WithFile("test.z80")
            .WithSaveFilePicker()
            .WithZ80SnapshotStore(snapshot => z80Snapshot = snapshot);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);

        z80Snapshot.ShouldNotBeNull();
        z80Snapshot.Header.HardwareMode.ShouldBe(HardwareMode.Spectrum128);
    }

    [AvaloniaFact]
    public async Task ShouldSave_SpectronSnapshotFile()
    {
        StateSnapshot? spectronSnapshot = null;

        var builder = _mainViewModelBuilder
            .WithFile("test.spectron")
            .WithSaveFilePicker()
            .WithStateSnapshotStore(snapshot => spectronSnapshot = snapshot);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);

        spectronSnapshot.ShouldNotBeNull();
        spectronSnapshot.ComputerType.ShouldBe(ComputerType.Spectrum128K);
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
}
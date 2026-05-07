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
    [AvaloniaFact]
    public async Task ShouldSave_SnaSnapshotFile()
    {
        var snapshotUri = new Uri("file:///path/file.sna");
        SnaFile? snaSnapshot = null;

        var builder = new MainViewModelBuilder()
            .WithSaveFilePicker(snapshotUri)
            .WithSnaSnapshotStore(snapshotUri, snapshot => snaSnapshot = snapshot);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);

        snaSnapshot.ShouldNotBeNull();
        snaSnapshot.Header128.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task ShouldSave_SzxSnapshotFile()
    {
        var snapshotUri = new Uri("file:///path/file.szx");
        SzxFile? szxSnapshot = null;

        var builder = new MainViewModelBuilder()
            .WithSaveFilePicker(snapshotUri)
            .WithSzxSnapshotStore(snapshotUri, snapshot => szxSnapshot = snapshot);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);

        szxSnapshot.ShouldNotBeNull();
        szxSnapshot.Header.MachineId.ShouldBe(SzxHeader.MachineId128K);
    }

    [AvaloniaFact]
    public async Task ShouldSave_Z80SnapshotFile()
    {
        var snapshotUri = new Uri("file:///path/file.z80");
        Z80File? z80Snapshot = null;

        var builder = new MainViewModelBuilder()
            .WithSaveFilePicker(snapshotUri)
            .WithZ80SnapshotStore(snapshotUri, snapshot => z80Snapshot = snapshot);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);

        z80Snapshot.ShouldNotBeNull();
        z80Snapshot.Header.HardwareMode.ShouldBe(HardwareMode.Spectrum128);
    }

    [AvaloniaFact]
    public async Task ShouldSave_SpectronSnapshotFile()
    {
        var snapshotUri = new Uri("file:///path/file.spectron");
        StateSnapshot? spectronSnapshot = null;

        var builder = new MainViewModelBuilder()
            .WithSaveFilePicker(snapshotUri)
            .WithStateSnapshotStore(snapshotUri, snapshot => spectronSnapshot = snapshot);

        var viewModel = builder.Build();
        viewModel.ChangeComputerTypeCommand.Execute(ComputerType.Spectrum128K);

        await viewModel.SaveFileCommand.ExecuteAsync(null);

        spectronSnapshot.ShouldNotBeNull();
        spectronSnapshot.ComputerType.ShouldBe(ComputerType.Spectrum128K);
    }
}
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Snapshot.Stores;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Files.Sna;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.Files.Z80;
using OldBit.Spectron.Services;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.Fixtures;

internal sealed class MainViewModelBuilder
{
    private readonly TestServiceProvider _servicesProvider;
    private IServiceCollection Services => _servicesProvider.Services;

    private Uri _fileUri = new("", UriKind.Relative);

    internal MainViewModelBuilder(TestServiceProvider servicesProvider)
    {
        _servicesProvider = servicesProvider;

        var environmentService = Substitute.For<IEnvironmentService>();

        environmentService.GetAppDataPath(Arg.Any<string>()).Returns(callInfo =>
        {
            var filePath = GetTestFilePath(callInfo.Arg<string>());

            return filePath;
        });

        Services.AddSingleton(environmentService);

        AudioManager.UseSilentAudioPlayer = true;
    }

    internal MainViewModelBuilder WithMessageDialogs(Action<string>? onMessage = null)
    {
        var mockMessageDialogs = Substitute.For<IMessageDialogs>();

        mockMessageDialogs
            .Error(Arg.Do<string>(m => onMessage?.Invoke(m)))
            .Returns(Task.CompletedTask);

        Services.AddSingleton(mockMessageDialogs);

        return this;
    }

    internal MainViewModelBuilder WithSaveFilePicker()
    {
        var mockStorageFile = Substitute.For<IStorageFile>();
        mockStorageFile.Path.Returns(_fileUri);

        var mockDialogProvider = Substitute.For<IFileDialogProvider>();

        mockDialogProvider
            .SaveFilePickerAsync(Arg.Any<FilePickerSaveOptions>())
            .Returns(Task.FromResult<IStorageFile?>(mockStorageFile));

        Services.AddSingleton(mockDialogProvider);

        return this;
    }

    internal MainViewModelBuilder WithOpenFilePicker()
    {
        var mockStorageFile = Substitute.For<IStorageFile>();
        mockStorageFile.Path.Returns(_fileUri);

        var mockDialogProvider = Substitute.For<IFileDialogProvider>();

        mockDialogProvider
            .OpenFilePickerAsync(Arg.Any<FilePickerOpenOptions>())
            .Returns(Task.FromResult<IReadOnlyList<IStorageFile>>([mockStorageFile]));

        Services.AddSingleton(mockDialogProvider);

        return this;
    }

    internal MainViewModelBuilder WithFile(string fileName)
    {
        var filePath = GetTestFilePath(fileName);
        _fileUri = new Uri(filePath);

        return this;
    }

    internal MainViewModelBuilder WithStateSnapshotStore(Action<StateSnapshot>? onSave = null, bool matchAnyName = false)
    {
        var mockSnapshotStore = Substitute.For<IStateSnapshotStore>();

        mockSnapshotStore.Save(
            matchAnyName ? Arg.Any<string>() : Arg.Is(_fileUri.LocalPath),
            Arg.Do<StateSnapshot>(snapshot => onSave?.Invoke(snapshot)));

        mockSnapshotStore.Load(Arg.Any<Stream>()).Returns(new StateSnapshotStore().Load(_fileUri.LocalPath));

        Services.AddSingleton(mockSnapshotStore);

        return this;
    }

    internal MainViewModelBuilder WithSnaSnapshotStore(Action<SnaFile>? onSave = null)
    {
        var mockSnapshotStore = Substitute.For<ISnaSnapshotStore>();

        mockSnapshotStore.Save(
            Arg.Is(_fileUri.LocalPath),
            Arg.Do<SnaFile>(snapshot => onSave?.Invoke(snapshot)));

        mockSnapshotStore.Load(Arg.Any<Stream>()).Returns(SnaFile.Load(_fileUri.LocalPath));

        Services.AddSingleton(mockSnapshotStore);

        return this;
    }

    internal MainViewModelBuilder WithSzxSnapshotStore(Action<SzxFile>? onSave = null)
    {
        var mockSnapshotStore = Substitute.For<ISzxSnapshotStore>();

        mockSnapshotStore.Save(
            Arg.Is(_fileUri.LocalPath),
            Arg.Do<SzxFile>(snapshot => onSave?.Invoke(snapshot)));

        mockSnapshotStore.Load(Arg.Any<Stream>()).Returns(SzxFile.Load(_fileUri.LocalPath));

        Services.AddSingleton(mockSnapshotStore);

        return this;
    }

    internal MainViewModelBuilder WithZ80SnapshotStore(Action<Z80File>? onSave = null)
    {
        var mockSnapshotStore = Substitute.For<IZ80SnapshotStore>();

        mockSnapshotStore.Save(
            Arg.Is(_fileUri.LocalPath),
            Arg.Do<Z80File>(snapshot => onSave?.Invoke(snapshot)));

        mockSnapshotStore.Load(Arg.Any<Stream>()).Returns(Z80File.Load(_fileUri.LocalPath));

        Services.AddSingleton(mockSnapshotStore);

        return this;
    }

    internal MainViewModel Build()
    {
        var viewModel = Services.BuildServiceProvider().GetRequiredService<MainViewModel>();

        viewModel.ScreenControl = Substitute.For<Avalonia.Controls.Image>();

        return viewModel;
    }

    private static string GetTestFilePath(string fileName) =>
        Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", fileName);
}
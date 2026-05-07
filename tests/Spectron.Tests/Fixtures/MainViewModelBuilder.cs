using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.Devices.Audio;
using OldBit.Spectron.Emulation.Snapshot.Stores;
using OldBit.Spectron.Emulation.State;
using OldBit.Spectron.Files.Sna;
using OldBit.Spectron.Files.Szx;
using OldBit.Spectron.Files.Z80;
using OldBit.Spectron.Logging;
using OldBit.Spectron.Services;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.Fixtures;

internal sealed class MainViewModelBuilder
{
    private readonly ServiceCollection _services = [];

    internal MainViewModelBuilder()
    {
        _services.AddServices();
        _services.AddViewModels();
        _services.AddEmulation();
        _services.AddDebugging();
        _services.AddLogging();
        _services.AddSingleton<ILogStore, InMemoryLogStore>();

        var mockApplicationDataService = Substitute.For<IApplicationDataService>();
        _services.AddSingleton(mockApplicationDataService);

        AudioManager.UseSilentAudioPlayer = true;
    }

    internal MainViewModelBuilder WithMessageDialogs(Action<string>? onMessage = null)
    {
        var mockMessageDialogs = Substitute.For<IMessageDialogs>();

        mockMessageDialogs
            .Error(Arg.Do<string>(m => onMessage?.Invoke(m)))
            .Returns(Task.CompletedTask);

        _services.AddSingleton(mockMessageDialogs);

        return this;
    }

    internal MainViewModelBuilder WithSaveFilePicker(Uri fileUri)
    {
        var mockStorageFile = Substitute.For<IStorageFile>();
        mockStorageFile.Path.Returns(fileUri);

        var mockDialogProvider = Substitute.For<IFileDialogProvider>();
        mockDialogProvider
            .SaveFilePickerAsync(Arg.Any<FilePickerSaveOptions>())
            .Returns(Task.FromResult<IStorageFile?>(mockStorageFile));

        _services.AddSingleton(mockDialogProvider);

        return this;
    }

    internal MainViewModelBuilder WithStateSnapshotStore(Uri fileUri, Action<StateSnapshot>? onSave = null)
    {
        var mockSnapshotStore = Substitute.For<IStateSnapshotStore>();
        mockSnapshotStore.Save(
            Arg.Is(fileUri.LocalPath),
            Arg.Do<StateSnapshot>(snapshot => onSave?.Invoke(snapshot)));

        _services.AddSingleton(mockSnapshotStore);

        return this;
    }

    internal MainViewModelBuilder WithSnaSnapshotStore(Uri fileUri, Action<SnaFile>? onSave = null)
    {
        var mockSnapshotStore = Substitute.For<ISnaSnapshotStore>();
        mockSnapshotStore.Save(
            Arg.Is(fileUri.LocalPath),
            Arg.Do<SnaFile>(snapshot => onSave?.Invoke(snapshot)));

        _services.AddSingleton(mockSnapshotStore);

        return this;
    }

    internal MainViewModelBuilder WithSzxSnapshotStore(Uri fileUri, Action<SzxFile>? onSave = null)
    {
        var mockSnapshotStore = Substitute.For<ISzxSnapshotStore>();
        mockSnapshotStore.Save(
            Arg.Is(fileUri.LocalPath),
            Arg.Do<SzxFile>(snapshot => onSave?.Invoke(snapshot)));

        _services.AddSingleton(mockSnapshotStore);

        return this;
    }

    internal MainViewModelBuilder WithZ80SnapshotStore(Uri fileUri, Action<Z80File>? onSave = null)
    {
        var mockSnapshotStore = Substitute.For<IZ80SnapshotStore>();
        mockSnapshotStore.Save(
            Arg.Is(fileUri.LocalPath),
            Arg.Do<Z80File>(snapshot => onSave?.Invoke(snapshot)));

        _services.AddSingleton(mockSnapshotStore);

        return this;
    }

    internal MainViewModel Build()
    {
        var viewModel = _services.BuildServiceProvider().GetRequiredService<MainViewModel>();

        viewModel.ScreenControl = Substitute.For<Avalonia.Controls.Image>();

        return viewModel;
    }
}
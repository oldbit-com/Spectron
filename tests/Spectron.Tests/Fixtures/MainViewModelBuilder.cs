using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OldBit.Spectron.Debugger.Extensions;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.DependencyInjection;
using OldBit.Spectron.Emulation.State;
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
        var mockStateSnapshotStore = Substitute.For<IStateSnapshotStore>();
        mockStateSnapshotStore.Save(Arg.Is(fileUri.LocalPath), Arg.Do<StateSnapshot>(snapshot => onSave?.Invoke(snapshot)));

        _services.AddSingleton(mockStateSnapshotStore);

        return this;
    }

    internal MainViewModel Build()
    {
        var viewModel = _services.BuildServiceProvider().GetRequiredService<MainViewModel>();

        viewModel.ScreenControl = Substitute.For<Avalonia.Controls.Image>();

        return viewModel;
    }
}
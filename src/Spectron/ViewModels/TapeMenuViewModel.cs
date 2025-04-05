using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Files;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Files.Extensions;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TapeMenuViewModel : ReactiveObject
{
    private bool _canStop;
    private bool _canPlay;
    private bool _canRewind;
    private bool _canEject;

    public ReactiveCommand<Unit, Unit> NewCommand { get; private set; }
    public ReactiveCommand<Unit, Task> InsertCommand { get; private set; }
    public ReactiveCommand<Unit, Task> SaveCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RewindCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> EjectCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ViewCommand { get; private set; }

    public Interaction<TapeViewModel, Unit?> ShowTapeView { get; }

    private readonly TapeManager _tapeManager;
    private readonly RecentFilesViewModel _recentFilesViewModel;

    public TapeMenuViewModel(TapeManager tapeManager, RecentFilesViewModel recentFilesViewModel)
    {
        _tapeManager = tapeManager;
        _recentFilesViewModel = recentFilesViewModel;

        NewCommand = ReactiveCommand.Create(NewTape);
        InsertCommand = ReactiveCommand.Create(InsertTape);

        SaveCommand = ReactiveCommand.Create(
            Save,
            this.WhenAnyValue(x => x.CanEject).Select(canEject => canEject));

       PlayCommand = ReactiveCommand.Create(
            () => { _tapeManager?.PlayTape(); },
            this.WhenAnyValue(x => x.CanPlay).Select(canPlay => canPlay));

        StopCommand = ReactiveCommand.Create(
            () => { _tapeManager?.StopTape(); },
            this.WhenAnyValue(x => x.CanStop).Select(canStop => canStop));

        RewindCommand = ReactiveCommand.Create(
            () => { _tapeManager?.RewindTape(); },
            this.WhenAnyValue(x => x.CanRewind).Select(canRewind => canRewind));

        EjectCommand = ReactiveCommand.Create(
            () => { _tapeManager?.EjectTape(); },
            this.WhenAnyValue(x => x.CanEject).Select(canEject => canEject));

        ViewCommand = ReactiveCommand.Create(
            OpenTapeView,
            this.WhenAnyValue(x => x.CanEject).Select(canEject => canEject));

        ShowTapeView = new Interaction<TapeViewModel, Unit?>();

        _tapeManager.TapeStateChanged += args => Dispatcher.UIThread.Post(() => { UpdateTapeState(args); });
    }

    private async Task Save()
    {
        var fileName = string.Empty;

        var fileType = FileTypeHelper.GetFileType(_recentFilesViewModel.CurrentFileName);

        if (fileType.IsTape())
        {
            fileName = _recentFilesViewModel.CurrentFileName;
        }

        var file = await FileDialogs.SaveTapeFileAsync(Path.GetFileNameWithoutExtension(fileName));

        if (file == null)
        {
            return;
        }

        fileType = FileTypeHelper.GetFileType(file.Path.LocalPath);

        if (fileType == FileType.Tap)
        {
            var tap = _tapeManager.Cassette.Content.ToTap();
            tap.Save(file.Path.LocalPath);
        }
        else
        {
            _tapeManager.Cassette.Content.Save(file.Path.LocalPath);
        }
    }

    private void UpdateTapeState(TapeStateEventArgs args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (args.Action)
            {
                case TapeAction.TapeInserted:
                    CanRewind = true;
                    CanPlay = true;
                    CanStop = false;
                    CanEject = true;

                    break;

                case TapeAction.TapeStopped:
                    CanRewind = true;
                    CanPlay = true;
                    CanStop = false;
                    CanEject = true;

                    break;

                case TapeAction.TapeStarted:
                    CanRewind = false;
                    CanPlay = false;
                    CanStop = true;
                    CanEject = true;

                    break;

                case TapeAction.TapeEjected:
                    CanRewind = false;
                    CanPlay = false;
                    CanStop = false;
                    CanEject = false;

                    break;
            }
        });
    }

    private async Task OpenTapeView()
    {
        using var viewModel = new TapeViewModel(_tapeManager);
        await ShowTapeView.Handle(viewModel);
    }

    private void NewTape() => _tapeManager.NewTape();

    private async Task InsertTape()
    {
        var files = await FileDialogs.OpenTapeFileAsync();

        if (files.Count == 0)
        {
            return;
        }

        _tapeManager.InsertTape(files[0].Path.LocalPath);
    }

    private bool CanStop
    {
        get => _canStop;
        set => this.RaiseAndSetIfChanged(ref _canStop, value);
    }

    private bool CanPlay
    {
        get => _canPlay;
        set => this.RaiseAndSetIfChanged(ref _canPlay, value);
    }

    private bool CanRewind
    {
        get => _canRewind;
        set => this.RaiseAndSetIfChanged(ref _canRewind, value);
    }

    private bool CanEject
    {
        get => _canEject;
        set => this.RaiseAndSetIfChanged(ref _canEject, value);
    }
}
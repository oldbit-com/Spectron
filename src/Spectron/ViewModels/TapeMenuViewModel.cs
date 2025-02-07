using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Tape;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TapeMenuViewModel : ReactiveObject
{
    public ReactiveCommand<Unit, Unit> NewCommand { get; private set; }
    public ReactiveCommand<Unit, Task> InsertCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RewindCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> EjectCommand { get; private set; }
    public ReactiveCommand<Unit, Task> ViewCommand { get; private set; }

    public Interaction<TapeViewModel, Unit?> ShowTapeView { get; }

    private readonly TapeManager _tapeManager;

    public TapeMenuViewModel(TapeManager tapeManager)
    {
        _tapeManager = tapeManager;

        var isTapeInserted = this.WhenAnyValue(x => x.IsTapeInserted).Select(inserted => inserted);
        var isTapePlaying = this.WhenAnyValue(x => x.IsTapePlaying).Select(playing => playing);
        var isTapeInsertedAndNotPlaying = this.WhenAnyValue(x => x.IsTapeInserted, x => x.IsTapePlaying, (inserted, playing) => inserted && !playing);

        NewCommand = ReactiveCommand.Create(NewTape);
        InsertCommand = ReactiveCommand.Create(InsertTape);
        PlayCommand = ReactiveCommand.Create(() => { _tapeManager?.PlayTape(); }, isTapeInsertedAndNotPlaying);
        StopCommand = ReactiveCommand.Create(() => { _tapeManager?.StopTape(); }, isTapePlaying);
        RewindCommand = ReactiveCommand.Create(Rewind, isTapeInserted);
        EjectCommand = ReactiveCommand.Create(() => { _tapeManager?.EjectTape(); }, isTapeInserted);
        ViewCommand = ReactiveCommand.Create(OpenTapeView, isTapeInserted);

        ShowTapeView = new Interaction<TapeViewModel, Unit?>();

        _tapeManager.TapeInserted += _ => Dispatcher.UIThread.Post(() => IsTapeInserted = true);
        _tapeManager.TapePlaying += _ => Dispatcher.UIThread.Post(() => IsTapePlaying = true);
        _tapeManager.TapeStopped += _ => Dispatcher.UIThread.Post(() => IsTapePlaying = false);
        _tapeManager.TapeEjected += _ => Dispatcher.UIThread.Post(() => IsTapeInserted = false);
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

    private void Rewind() { }

    private bool _isTapeInserted;
    private bool IsTapeInserted
    {
        get => _isTapeInserted;
        set => this.RaiseAndSetIfChanged(ref _isTapeInserted, value);
    }

    private bool _isTapePlaying;
    private bool IsTapePlaying
    {
        get => _isTapePlaying;
        set => this.RaiseAndSetIfChanged(ref _isTapePlaying, value);
    }
}
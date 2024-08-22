using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Emulation.Tape;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TapeMenuViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Task> InsertCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RewindCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> EjectCommand { get; private set; }

    private TapeManager? _tapeManager;

    public TapeMenuViewModel()
    {
        var isTapeInserted = this.WhenAnyValue(x => x.IsTapeInserted).Select(inserted => inserted);
        var isTapePlaying = this.WhenAnyValue(x => x.IsTapePlaying).Select(playing => playing);
        var isTapeInsertedAndNotPlaying = this.WhenAnyValue(x => x.IsTapeInserted, x => x.IsTapePlaying, (inserted, playing) => inserted && !playing);

        InsertCommand = ReactiveCommand.Create(Insert);
        PlayCommand = ReactiveCommand.Create(() => { TapeManager?.PlayTape(); }, isTapeInsertedAndNotPlaying);
        StopCommand = ReactiveCommand.Create(() => { TapeManager?.StopTape(); }, isTapePlaying);
        RewindCommand = ReactiveCommand.Create(Rewind, isTapeInserted);
        EjectCommand = ReactiveCommand.Create(() => { TapeManager?.EjectTape(); }, isTapeInserted);
    }

    private async Task Insert()
    {
        var files = await FileDialogs.OpenTapeFileAsync();

        if (files.Count == 0)
        {
            return;
        }

        TapeManager?.TryInsertTape(files[0].Path.LocalPath);
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

    internal TapeManager? TapeManager
    {
        get  => _tapeManager;
        set
        {
            _tapeManager = value;
            if (_tapeManager == null)
            {
                return;
            }

            _tapeManager.TapeInserted += _ => IsTapeInserted = true;
            _tapeManager.TapePlaying += _ => IsTapePlaying = true;
            _tapeManager.TapeStopped += _ => IsTapePlaying = false;
            _tapeManager.TapeEjected += _ => IsTapeInserted = false;
        }
    }
}
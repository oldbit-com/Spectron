using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OldBit.Spectral.Dialogs;
using OldBit.Spectral.Emulation.Tape;
using ReactiveUI;

namespace OldBit.Spectral.ViewModels;

public class TapeMenuViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Task> InsertCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RewindCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> EjectCommand { get; private set; }

    internal TapeManager? TapeManager { get; set; }

    public TapeMenuViewModel()
    {
        var isTapeInserted = this.WhenAnyValue(x => x.IsTapeInserted).Select(inserted => inserted);
        var isTapePlaying = this.WhenAnyValue(x => x.IsTapePlaying).Select(playing => playing);
        var isTapeInsertedAndNotPlaying = this.WhenAnyValue(x => x.IsTapeInserted, x => x.IsTapePlaying, (inserted, playing) => inserted && !playing);

        InsertCommand = ReactiveCommand.Create(Insert);
        PlayCommand = ReactiveCommand.Create(Play, isTapeInsertedAndNotPlaying);
        StopCommand = ReactiveCommand.Create(Stop, isTapePlaying);
        RewindCommand = ReactiveCommand.Create(Rewind, isTapeInserted);
        EjectCommand = ReactiveCommand.Create(Eject, isTapeInserted);
    }

    private async Task Insert()
    {
        var files = await FileDialogs.OpenTapeFileAsync();
        if (files.Count == 0)
        {
            return;
        }

        if (TapeManager?.TryInsertTape(files[0].Path.LocalPath) == true)
        {
            IsTapeInserted = true;
        }
    }

    private void Play()
    {
        TapeManager?.PlayTape();

        IsTapePlaying = true;
    }

    private void Stop()
    {
        IsTapePlaying = false;
    }

    private void Rewind()
    {

    }

    private void Eject()
    {
        TapeManager?.EjectTape();

        IsTapeInserted = false;
        IsTapePlaying = false;
    }

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
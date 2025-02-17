using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Threading;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Files.Tzx.Extensions;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TapeViewModel : ReactiveObject, IDisposable
{
    private readonly TapeManager _tapeManager;

    private bool _canStop;
    private bool _canPlay;
    private bool _canRewind;

    public ObservableCollection<TapeBlockViewModel> Blocks { get; } = [];

    public ReactiveCommand<Unit, Unit> RewindCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PlayCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> EjectCommand { get; private set; }

    public TapeViewModel(TapeManager tapeManager)
    {
        _tapeManager = tapeManager;

        _tapeManager.Cassette.BlockSelected += CassetteOnCassettePositionChanged;
        _tapeManager.Cassette.EndOfTape += CassetteOnEndOfTape;
        _tapeManager.TapeStateChanged += TapeManagerOnTapeStateChanged;

        RewindCommand = ReactiveCommand.Create(Rewind);
        PlayCommand = ReactiveCommand.Create(Play);
        StopCommand = ReactiveCommand.Create(Stop);
        EjectCommand = ReactiveCommand.Create(Eject);

        CanRewind = _tapeManager.IsTapeLoaded;
        CanPlay = _tapeManager is { IsTapeLoaded: true, IsPlaying: false };
        CanStop = _tapeManager is { IsTapeLoaded: true, IsPlaying: true };

        PopulateBlocks();
    }

    private void TapeManagerOnTapeStateChanged(TapeStateEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (e.Action)
            {
                case TapeAction.TapeStopped:
                    CanRewind = true;
                    CanPlay = true;
                    CanStop = false;

                    break;

                case TapeAction.TapeStarted:
                    CanRewind = false;
                    CanPlay = false;
                    CanStop = true;

                    break;

                case TapeAction.TapeEjected:
                    CanPlay = false;
                    CanStop = false;
                    CanRewind = false;

                    break;
            }
        });
    }

    private void CassetteOnCassettePositionChanged(BlockSelectedEventArgs e) =>
        Dispatcher.UIThread.Post(() => MarkActiveBlock(e.Position));

    private void CassetteOnEndOfTape(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(() => MarkActiveBlock(Blocks.Count - 1));

    private void PopulateBlocks()
    {
        for (var i = 0; i <  _tapeManager.Cassette.Content.Blocks.Count; i++)
        {
            var block = _tapeManager.Cassette.Content.Blocks[i];

            Blocks.Add(new TapeBlockViewModel(i + 1, block.GetBlockName(), block.ToString() ?? string.Empty));
        }

        Blocks.Add(new TapeBlockViewModel(null, "", "<end of tape>"));

        MarkActiveBlock(_tapeManager.IsPlaying ? _tapeManager.Cassette.Position - 1 : _tapeManager.Cassette.Position);
    }

    private void MarkActiveBlock(int position)
    {
        Blocks.Where(b => b.IsSelected).ForEach(b => b.IsSelected = false);

        if (position >= Blocks.Count)
        {
            Blocks[^1].IsSelected = true;
        }
        else
        {
            Blocks[position].IsSelected = true;
        }
    }

    private void Rewind() => _tapeManager.RewindTape();

    private void Eject()
    {
        _tapeManager.EjectTape();
        Blocks.Clear();
    }

    private void Play() => _tapeManager.PlayTape();

    private void Stop() => _tapeManager.StopTape();

    public void SetActiveBlock(int position) => _tapeManager.Cassette.SetPosition(position);

    public bool CanStop
    {
        get => _canStop;
        set => this.RaiseAndSetIfChanged(ref _canStop, value);
    }

    public bool CanPlay
    {
        get => _canPlay;
        set => this.RaiseAndSetIfChanged(ref _canPlay, value);
    }

    public bool CanRewind
    {
        get => _canRewind;
        set => this.RaiseAndSetIfChanged(ref _canRewind, value);
    }

    public void Dispose()
    {
        _tapeManager.Cassette.BlockSelected -= CassetteOnCassettePositionChanged;
        _tapeManager.Cassette.EndOfTape -= CassetteOnEndOfTape;
        _tapeManager.TapeStateChanged -= TapeManagerOnTapeStateChanged;

        GC.SuppressFinalize(this);
    }
}
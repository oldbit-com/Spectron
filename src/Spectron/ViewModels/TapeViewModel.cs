using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Files.Tzx.Extensions;

namespace OldBit.Spectron.ViewModels;

public partial class TapeViewModel : ObservableObject, IDisposable
{
    private readonly TapeManager _tapeManager;
    private readonly Timer _tapeProgressTimer;

    [ObservableProperty]
    private bool _canStop;

    [ObservableProperty]
    private bool _canPlay;

    [ObservableProperty]
    private bool _canRewind;

    [ObservableProperty]
    private bool _canEject;

    [ObservableProperty]
    private double _progress;

    public ObservableCollection<TapeBlockViewModel> Blocks { get; } = [];

    public TapeViewModel(TapeManager tapeManager)
    {
        _tapeManager = tapeManager;

        _tapeProgressTimer = new Timer(200);
        _tapeProgressTimer.AutoReset = true;
        _tapeProgressTimer.Elapsed += TapeProgressUpdate;
        _tapeProgressTimer.Start();

        _tapeManager.Cassette.BlockSelected += CassetteOnPositionChanged;
        _tapeManager.Cassette.EndOfTape += CassetteOnEndOfTape;
        _tapeManager.StateChanged += TapeOnStateChanged;

        CanRewind = _tapeManager.IsTapeLoaded;
        CanPlay = _tapeManager is { IsTapeLoaded: true, IsPlaying: false };
        CanStop = _tapeManager is { IsTapeLoaded: true, IsPlaying: true };
        CanEject = _tapeManager.IsTapeLoaded;

        PopulateBlocks();
    }

    [RelayCommand]
    private void Rewind() => _tapeManager.RewindTape();

    [RelayCommand]
    private void Play() => _tapeManager.PlayTape();

    [RelayCommand]
    private void Stop() => _tapeManager.StopTape();

    [RelayCommand]
    private void Eject()
    {
        _tapeManager.EjectTape();
        Blocks.Clear();
    }

    private void TapeProgressUpdate(object? sender, ElapsedEventArgs e)
    {
        if (!_tapeManager.IsPlaying)
        {
            return;
        }

        Dispatcher.UIThread.Post(() => Progress = _tapeManager.BlockReadProgressPercentage);
    }

    private void TapeOnStateChanged(TapeStateEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (e.Action)
            {
                case TapeAction.Stopped:
                    CanRewind = true;
                    CanPlay = true;
                    CanStop = false;
                    CanEject = true;

                    break;

                case TapeAction.Started:
                    CanRewind = false;
                    CanPlay = false;
                    CanStop = true;
                    CanEject = true;

                    break;

                case TapeAction.Ejected:
                    CanPlay = false;
                    CanStop = false;
                    CanRewind = false;
                    CanEject = false;

                    break;
            }
        });
    }

    private void CassetteOnPositionChanged(BlockSelectedEventArgs e) =>
        Dispatcher.UIThread.Post(() =>
        {
            Progress = 0;
            MarkActiveBlock(e.Position);
        });

    private void CassetteOnEndOfTape(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(() =>
        {
            Progress = 0;
            MarkActiveBlock(Blocks.Count - 1);
        });

    private void PopulateBlocks()
    {
        for (var i = 0; i < _tapeManager.Cassette.Content.Blocks.Count; i++)
        {
            var block = _tapeManager.Cassette.Content.Blocks[i];

            Blocks.Add(new TapeBlockViewModel(i + 1, block.GetBlockName(), block.ToString() ?? string.Empty));
        }

        Blocks.Add(new TapeBlockViewModel(null, "", "<end of tape>"));

        MarkActiveBlock(_tapeManager.IsPlaying ? _tapeManager.Cassette.Position - 1 : _tapeManager.Cassette.Position);
    }

    private void MarkActiveBlock(int position)
    {
        if (Blocks.Count == 0)
        {
            return;
        }

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

    public void SetActiveBlock(int position) => _tapeManager.Cassette.SetPosition(position);

    public void Dispose()
    {
        _tapeProgressTimer.Stop();
        _tapeProgressTimer.Dispose();

        _tapeManager.Cassette.BlockSelected -= CassetteOnPositionChanged;
        _tapeManager.Cassette.EndOfTape -= CassetteOnEndOfTape;
        _tapeManager.StateChanged -= TapeOnStateChanged;

        GC.SuppressFinalize(this);
    }
}
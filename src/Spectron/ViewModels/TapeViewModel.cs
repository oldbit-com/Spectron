using System;
using System.Collections.ObjectModel;
using System.Linq;
using OldBit.Spectron.Emulation.Tape;
using OldBit.Spectron.Extensions;
using OldBit.ZX.Files.Tzx.Extensions;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TapeBlock(string index, string name, string data) : ViewModelBase
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public string Index { get; init; } = index;
    public string Name { get; init; } = name;
    public string Data { get; init; } = data;
}

public class TapeViewModel : ViewModelBase, IDisposable
{
    private readonly TapeManager _tapeManager;
    public ObservableCollection<TapeBlock> Blocks { get; } = [];

    public TapeViewModel()
    {
        Blocks.Add(new TapeBlock("1", "1", "1") { IsSelected = true });
    }

    public TapeViewModel(TapeManager tapeManager)
    {
        _tapeManager = tapeManager;
        _tapeManager.Cassette.BlockSelected += CassetteOnCassettePositionChanged;
        _tapeManager.Cassette.EndOfTape += CassetteOnEndOfTape;

        for (var i = 0; i <  _tapeManager.Cassette.Content.Blocks.Count; i++)
        {
            var block = _tapeManager.Cassette.Content.Blocks[i];

            Blocks.Add(new TapeBlock((i + 1).ToString(), block.GetBlockName(), block.ToString() ?? string.Empty));
        }

        Blocks.Add(new TapeBlock("", "", "<end of tape>"));

        SelectBlock(_tapeManager.IsPlaying ? _tapeManager.Cassette.Position - 1 : _tapeManager.Cassette.Position);
    }

    private void CassetteOnCassettePositionChanged(BlockSelectedEventArgs e) => SelectBlock(e.Position);

    private void CassetteOnEndOfTape(object? sender, EventArgs e) => SelectBlock(Blocks.Count - 1);

    private void SelectBlock(int position)
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

    public void Dispose()
    {
        _tapeManager.Cassette.BlockSelected -= CassetteOnCassettePositionChanged;
        _tapeManager.Cassette.EndOfTape -= CassetteOnEndOfTape;

        GC.SuppressFinalize(this);
    }
}
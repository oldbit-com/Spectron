using System;
using System.Collections.ObjectModel;
using OldBit.Spectron.Emulation.Tape;
using OldBit.ZX.Files.Tzx.Extensions;

namespace OldBit.Spectron.ViewModels;

public record TapeBlock(int Index, string Name, string Data);

public class TapeViewModel : ViewModelBase, IDisposable
{
    private readonly TapeManager _tapeManager;
    public ObservableCollection<TapeBlock> Blocks { get; } = [];

    public TapeViewModel(TapeManager tapeManager)
    {
        _tapeManager = tapeManager;

        _tapeManager.TapeFile.TapeBlockSelected += TapeFileOnTapeBlockSelected;

        for (var i = 0; i <  _tapeManager.TapeFile.CurrentFile.Blocks.Count; i++)
        {
            var block = _tapeManager.TapeFile.CurrentFile.Blocks[i];
            Blocks.Add(new TapeBlock(i + 1, block.GetBlockName(), block.ToString() ?? string.Empty));
        }
    }

    private void TapeFileOnTapeBlockSelected(TapeBlockSelectedEventArgs e)
    {
        // Handle block selected event
    }

    public void Dispose()
    {
        _tapeManager.TapeFile.TapeBlockSelected -= TapeFileOnTapeBlockSelected;
    }
}
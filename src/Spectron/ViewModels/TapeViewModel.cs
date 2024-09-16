using System.Collections.ObjectModel;
using OldBit.Spectron.Emulation.Tape;
using OldBit.ZX.Files.Tzx.Extensions;

namespace OldBit.Spectron.ViewModels;

public record TapeBlock(int Index, string Name, string Data);

public class TapeViewModel : ViewModelBase
{
    private readonly TapeManager _tapeManager;
    public ObservableCollection<TapeBlock> Blocks { get; } = [];

    // TODO: Delete, only for design-time data
    public TapeViewModel()
    {
        Blocks.Add(new TapeBlock(1, "Block 1", "Data 1"));
        Blocks.Add(new TapeBlock(2, "Block 2", "Data 2"));
        Blocks.Add(new TapeBlock(3, "Block 3", "Data 3"));
    }

    public TapeViewModel(TapeManager tapeManager)
    {
        _tapeManager = tapeManager;

        for (var i = 0; i <  _tapeManager.Tape.File.Blocks.Count; i++)
        {
            var block = _tapeManager.Tape.File.Blocks[i];
            Blocks.Add(new TapeBlock(i + 1, block.GetBlockName(), block.ToString() ?? string.Empty));
        }
    }
}
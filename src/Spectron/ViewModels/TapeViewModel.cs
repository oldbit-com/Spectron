using System.Collections.ObjectModel;

namespace OldBit.Spectron.ViewModels;

public record TapeBlock(string Name, string Data);

public class TapeViewModel : ViewModelBase
{
    public ObservableCollection<TapeBlock> Blocks { get; } = [];

    public TapeViewModel()
    {
        Blocks.Add(new TapeBlock("Block 1", "Data 1"));
        Blocks.Add(new TapeBlock("Block 2", "Data 2"));
        Blocks.Add(new TapeBlock("Block 3", "Data 3"));
    }
}
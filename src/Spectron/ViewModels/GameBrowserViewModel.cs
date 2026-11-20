using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OldBit.Spectron.ViewModels;

public partial class GameBrowserViewModel : ObservableObject
{
    public ObservableCollection<GameViewModel> Games { get; } = [];

    public IAsyncRelayCommand SearchCommand { get; }

    [ObservableProperty]
    public partial GameViewModel? SelectedGame { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public GameBrowserViewModel()
    {
        SearchCommand = new AsyncRelayCommand(Search);

        Games.Add(new GameViewModel
        {
            Name = "Game 1",
            Publisher = "Publisher 1",
            Year = "2020",
            Genre = "Arcade Game: Maze",
            Availability = "Distribution denied - still for sale",
            Data = File.ReadAllBytes("/Users/voytas/Downloads/PrizeThe.scr"),
        });

        Games.Add(new GameViewModel
        {
            Name = "Writer's Digest Prize Draw Very Long Name Even Longer",
            Publisher = "Publisher 2",
            Year = "2021",
            Genre = "Arcade Game: Maze",
            Availability = "Available",
            Data = File.ReadAllBytes("/Users/voytas/Downloads/PrizeThe.scr"),
        });

        Games.Add(new GameViewModel
        {
            Name = "Game 3",
            Publisher = "Publisher 3",
            Year = "2022",
            Genre = "Arcade Game: Maze",
            Availability = "Distribution denied",
            Data = File.ReadAllBytes("/Users/voytas/Downloads/PrizeThe.scr"),
        });
    }

    private async Task Search(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return;
        }

        //await _zxDbClient.SearchByTitle(SearchText, cancellationToken);
    }
}
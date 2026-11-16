using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public partial class GameBrowserViewModel : ObservableObject
{
    public ObservableCollection<GameViewModel> Games { get; } = [];

    [ObservableProperty]
    public partial GameViewModel? SelectedGame { get; set; }

    public GameBrowserViewModel()
    {
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
}
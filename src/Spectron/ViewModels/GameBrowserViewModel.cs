using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public class GameBrowserViewModel : ObservableObject
{
    public ObservableCollection<GameViewModel> Games { get; } = [];

    public GameBrowserViewModel()
    {
        Games.Add(new GameViewModel
        {
            Name = "Game 1",
            Publisher = "Publisher 1",
            Year = "2020"
        });

        Games.Add(new GameViewModel
        {
            Name = "Game 2",
            Publisher = "Publisher 2",
            Year = "2021"
        });

        Games.Add(new GameViewModel
        {
            Name = "Game 3",
            Publisher = "Publisher 3",
            Year = "2022"
        });
    }
}
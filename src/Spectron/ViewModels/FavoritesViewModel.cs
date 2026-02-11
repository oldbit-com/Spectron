using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    [ObservableProperty]
    private FavoriteItemViewModel? _selectedItem;

    public ObservableCollection<FavoriteItemViewModel> Items { get; }

    public FavoritesViewModel()
    {
        Items =
        [
            new FavoriteItemViewModel("Favorites",
            [
                new FavoriteItemViewModel("Games",
                [
                    new FavoriteItemViewModel("Manic Miner"),
                    new FavoriteItemViewModel("The Prize"),
                    new FavoriteItemViewModel("Bruce Lee")
                ]),
                new FavoriteItemViewModel("Demos",
                [
                    new FavoriteItemViewModel("Demo 1"),
                    new FavoriteItemViewModel("Demo 2"),
                    new FavoriteItemViewModel("Demo 3")
                ]),
                new FavoriteItemViewModel("Some other item 1"),
                new FavoriteItemViewModel("Some other item 2"),
            ]),
        ];

        // var moth = Items.Last().SubNodes?.Last();
        // if (moth!=null) SelectedItems.Add(moth);
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    [ObservableProperty]
    private FavoriteItemViewModel? _selectedItem;

    public ObservableCollection<FavoriteItemViewModel> Nodes { get; } = [];

    public List<FavoriteProgram> Favorites
    {
        get;
        set
        {
            field = value;
            UpdateFavorites();
        }
    } = [];

    public void Opening(ItemCollection menuItems)
    {
        while (menuItems.Count > 1)
        {
            menuItems.RemoveAt(1);
        }

        if (Nodes.Count == 0 || Nodes[0].Nodes is { Count: 0 })
        {
            return;
        }

        menuItems.Add(new Separator());

        AddFavoriteItems(menuItems, Nodes[0].Nodes!);
    }

    private void UpdateFavorites()
    {
        Nodes.Clear();
        Nodes.Add(new FavoriteItemViewModel { Title = "Favorites", IsFolder = true, IsReadOnly = true });

        AddFavorites(Favorites, Nodes[0]);
    }

    private void AddFavorites(List<FavoriteProgram> favorites, FavoriteItemViewModel parent)
    {
        foreach (var favorite in favorites)
        {
            if (favorite.IsFolder)
            {
                var folder = new FavoriteItemViewModel { Title = favorite.Title, IsFolder = true };
                parent.Nodes.Add(folder);

                AddFavorites(favorite.Favorites, folder);

                continue;
            }

            parent.Nodes.Add(new FavoriteItemViewModel { Title = favorite.Title });
        }
    }

    [RelayCommand]
    private async Task OpenFavorite(FavoriteItemViewModel favorite)
    {
        Console.WriteLine("Opening: " + favorite.Title);
    }

    private void AddFavoriteItems(ItemCollection menuItems, IEnumerable<FavoriteItemViewModel> favorites)
    {
        foreach (var favorite in favorites)
        {
            var favoriteMenuItem = new MenuItem
            {
                Header = favorite.Title,
            };

            if (favorite is { IsFolder: true, Nodes.Count: > 0 })
            {
                AddFavoriteItems(favoriteMenuItem.Items, favorite.Nodes);
            }
            else
            {
                favoriteMenuItem.Command = OpenFavoriteCommand;
                favoriteMenuItem.CommandParameter = favorite;
            }

            menuItems.Add(favoriteMenuItem);
        }
    }
}
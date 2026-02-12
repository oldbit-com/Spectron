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

    private static void AddFavorites(List<FavoriteProgram> favorites, FavoriteItemViewModel parent)
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

            parent.Nodes.Add(new FavoriteItemViewModel { Title = favorite.Title, IsFile = true });
        }
    }

    [RelayCommand]
    private async Task OpenFavorite(FavoriteItemViewModel favorite)
    {
        Console.WriteLine("Opening: " + favorite.Title);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteRemove))]
    private void RemoveItem()
    {
        if (SelectedItem is null)
        {
            return;
        }

        Remove(Nodes, SelectedItem);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteInsert))]
    private void InsertFolder() => InsertItem(new FavoriteItemViewModel { Title = "New Folder", IsFolder = true });

    [RelayCommand(CanExecute = nameof(CanExecuteInsert))]
    private void InsertItem() => InsertItem(new FavoriteItemViewModel { Title = "New File", IsFile = true });

    private void InsertItem(FavoriteItemViewModel item)
    {
        if (SelectedItem is null || !SelectedItem.IsFolder)
        {
            return;
        }

        SelectedItem.Nodes.Add(item);

        SelectedItem = item;
    }

    private void AddFavoriteItems(ItemCollection menuItems, IEnumerable<FavoriteItemViewModel> favorites)
    {
        foreach (var favorite in favorites)
        {
            var favoriteMenuItem = new MenuItem
            {
                Header = favorite.Title,
            };

            if (favorite.IsFolder)
            {
                if (favorite.Nodes.Count == 0)
                {
                    continue;
                }

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

    private static void Remove(ObservableCollection<FavoriteItemViewModel> nodes, FavoriteItemViewModel item)
    {
        if (nodes.Remove(item))
        {
            return;
        }

        foreach (var node in nodes)
        {
            Remove(node.Nodes, item);
        }
    }

    private bool CanExecuteRemove => SelectedItem is not null && !SelectedItem.IsReadOnly;
    private bool CanExecuteInsert => SelectedItem is not null && SelectedItem.IsFolder;
}
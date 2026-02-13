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
    private FavoriteItemViewModel? _cutItem;
    private FavoriteItemViewModel? _copyItem;

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
        Nodes.Add(new FavoriteItemViewModel { Title = "Favorites", IsFolder = true, IsRoot = true });

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

    [RelayCommand(CanExecute = nameof(CanExecuteCutCopy))]
    private void CutSelectedItem()
    {
        _cutItem?.IsCutItem = false;
        _cutItem = SelectedItem;
        _cutItem?.IsCutItem = true;
        _copyItem = null;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCutCopy))]
    private void CopySelectedItem()
    {
        _cutItem?.IsCutItem = false;
        _cutItem = null;
        _copyItem = SelectedItem;
    }

    [RelayCommand(CanExecute = nameof(CanExecutePaste))]
    private void PasteItem()
    {
        if (SelectedItem is null)
        {
            return;
        }

        if (_cutItem != null)
        {
            CutItem(_cutItem, SelectedItem);
        }
        else if (_copyItem != null)
        {
            CopyItem(_copyItem, SelectedItem);
        }
    }

    private void InsertItem(FavoriteItemViewModel item)
    {
        if (SelectedItem is null || !SelectedItem.IsFolder)
        {
            return;
        }

        SelectedItem.Nodes.Add(item);
        SelectedItem = item;
    }

    private void CopyItem(FavoriteItemViewModel copyItem, FavoriteItemViewModel selectedItem)
    {
        var copy = copyItem.Clone();
        copy.Title = $"{copyItem.Title} - copy";

        if (selectedItem.IsFolder && !copyItem.IsFolder)
        {
            selectedItem.Nodes.Insert(0, copy);
        }
        else
        {
            var parent = FindParent(Nodes, selectedItem);

            if (parent == null)
            {
                return;
            }

            var index = parent.Nodes.IndexOf(selectedItem);
            parent.Nodes.Insert(index + 1, copy);
        }

        SelectedItem = copy;
    }

    private void CutItem(FavoriteItemViewModel cutItem, FavoriteItemViewModel selectedItem)
    {
        var cutItemParent = FindParent(Nodes, cutItem);
        var selectedItemParent = FindParent(Nodes, selectedItem);

        if (cutItemParent == null || selectedItem.IsFile && cutItemParent == selectedItemParent)
        {
            return;
        }

        if (IsChildOfAny(selectedItem, cutItem.Nodes))
        {
            return;
        }

        var cutItemIndex = cutItemParent.Nodes.IndexOf(cutItem);
        cutItemParent.Nodes.RemoveAt(cutItemIndex);

        if (selectedItem.IsFolder)
        {
            selectedItem.Nodes.Insert(0, cutItem);
        }
        else
        {
            selectedItemParent?.Nodes.Insert(selectedItemParent.Nodes.IndexOf(selectedItem) + 1, cutItem);
        }

        SelectedItem = cutItem;

        _cutItem?.IsCutItem = false;
        _cutItem = null;
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

    private static FavoriteItemViewModel? FindParent(IEnumerable<FavoriteItemViewModel> nodes, FavoriteItemViewModel item)
    {
        foreach (var node in nodes)
        {
            if (node.Nodes.Contains(item))
            {
                return node;
            }

            var parent = FindParent(node.Nodes, item);

            if (parent != null)
            {
                return parent;
            }
        }

        return null;
    }

    private static bool IsChildOfAny(FavoriteItemViewModel item, IEnumerable<FavoriteItemViewModel> nodes)
    {
        foreach (var node in nodes)
        {
            if (node == item)
            {
                return true;
            }

            if (IsChildOfAny(item, node.Nodes))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanExecuteRemove => SelectedItem is not null && !SelectedItem.IsRoot;
    private bool CanExecuteInsert => SelectedItem is not null && (SelectedItem.IsFolder || SelectedItem.IsRoot);
    private bool CanExecuteCutCopy => SelectedItem is not null && !SelectedItem.IsRoot;
    private bool CanExecutePaste => SelectedItem is not null && (_cutItem != null || _copyItem != null);
}
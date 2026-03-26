using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Extensions;
using OldBit.Spectron.Messages;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    private FavoriteItemViewModel? _cutItem;
    private FavoriteItemViewModel? _copyItem;

    [ObservableProperty]
    private FavoriteItemViewModel? _selectedItem;

    public ObservableCollection<FavoriteItemViewModel> Nodes { get; } = [];

    public TreeView? FavoritesTreeView { get; set; }

    public FavoritePrograms Favorites
    {
        private get;
        set
        {
            field = value;
            RefreshFavorites();
        }
    } = new();

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

    public void Opening(NativeMenu favoritesMenu)
    {
        while (favoritesMenu.Items.Count > 1)
        {
            favoritesMenu.Items.RemoveAt(1);
        }

        if (Nodes.Count == 0 || Nodes[0].Nodes is { Count: 0 })
        {
            return;
        }

        favoritesMenu.Items.Add(new NativeMenuItemSeparator());

        AddFavoriteItems(favoritesMenu.Items, Nodes[0].Nodes!);
    }

    private FavoritePrograms GetFavorites()
    {
        return new FavoritePrograms
        {
            Items = Convert(Nodes[0].Nodes)
        };

        static List<FavoriteProgram> Convert(IEnumerable<FavoriteItemViewModel> nodes)
        {
            var result = new List<FavoriteProgram>();

            foreach (var node in nodes)
            {
                var favorite = node.ToFavoriteProgram();

                if (node.IsFolder)
                {
                    favorite.Items.AddRange(Convert(node.Nodes));
                }

                result.Add(favorite);
            }

            return result;
        }
    }

    public void UpdateFavorites()
    {
        var favorites = GetFavorites();
        WeakReferenceMessenger.Default.Send(new UpdateFavoritesMessage(favorites));
    }

    private void RefreshFavorites()
    {
        Nodes.Clear();
        Nodes.Add(new FavoriteItemViewModel { Title = "Favorites", IsFolder = true, IsRoot = true });

        AddFavorites(Favorites.Items, Nodes[0]);
    }

    private static void AddFavorites(List<FavoriteProgram> favorites, FavoriteItemViewModel parent)
    {
        foreach (var favorite in favorites)
        {
            if (favorite.IsFolder)
            {
                var folder = new FavoriteItemViewModel { Title = favorite.Title, IsFolder = true };
                parent.Nodes.Add(folder);

                AddFavorites(favorite.Items, folder);

                continue;
            }

            parent.Nodes.Add(new FavoriteItemViewModel
            {
                Title = favorite.Title,
                Path = favorite.Path,
                IsFile = true,
                SettingsViewModel = new FavoriteSettingsViewModel
                (
                    favorite.ComputerType,
                    favorite.JoystickType,
                    favorite.MouseType,
                    favorite.TapeLoadSpeed,
                    favorite.IsUlaPlusEnabled,
                    favorite.IsAyEnabled,
                    favorite.IsInterface1Enabled,
                    favorite.IsBeta128Enabled,
                    favorite.IsDivMmcEnabled
                )
            });
        }
    }

    [RelayCommand]
    private static void OpenFavorite(FavoriteItemViewModel favorite) =>
        WeakReferenceMessenger.Default.Send(new OpenFavoriteMessage(favorite.ToFavoriteProgram()));

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

    [RelayCommand(CanExecute = nameof(CanMoveItemUp))]
    private void MoveItemUp()
    {
        if (SelectedItem is null)
        {
            return;
        }

        var parentNode = FindParent(Nodes, SelectedItem)?.Nodes;
        var nodeIndex = parentNode?.IndexOf(SelectedItem) ?? - 1;

        if (parentNode is null || nodeIndex < 0)
        {
            return;
        }

        parentNode.Move(nodeIndex, nodeIndex - 1);
        FavoritesTreeView?.FindContainer(SelectedItem)?.Focus();
    }

    [RelayCommand(CanExecute = nameof(CanMoveItemDown))]
    private void MoveItemDown()
    {
        if (SelectedItem is null)
        {
            return;
        }

        var parentNode = FindParent(Nodes, SelectedItem)?.Nodes;
        var nodeIndex = parentNode?.IndexOf(SelectedItem) ?? -1;

        if (parentNode is null || nodeIndex < 0 || nodeIndex >= parentNode.Count - 1)
        {
            return;
        }

        parentNode.Move(nodeIndex, nodeIndex + 1);
        FavoritesTreeView?.FindContainer(SelectedItem)?.Focus();
    }

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

        if (_cutItem is not null)
        {
            CutItem(_cutItem, SelectedItem);
        }
        else if (_copyItem is not null)
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

            if (parent is null)
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

        if (cutItemParent is null || selectedItem.IsFile && cutItemParent == selectedItemParent)
        {
            return;
        }

        if (IsDescendantOf(cutItem.Nodes, selectedItem))
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
            if (string.IsNullOrWhiteSpace(favorite.Title))
            {
                continue;
            }

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
                if (string.IsNullOrWhiteSpace(favorite.Path))
                {
                    continue;
                }

                favoriteMenuItem.Command = OpenFavoriteCommand;
                favoriteMenuItem.CommandParameter = favorite;
            }

            menuItems.Add(favoriteMenuItem);
        }
    }

    private void AddFavoriteItems(IList<NativeMenuItemBase> menuItems, IEnumerable<FavoriteItemViewModel> favorites)
    {
        foreach (var favorite in favorites)
        {
            if (string.IsNullOrWhiteSpace(favorite.Title))
            {
                continue;
            }

            var favoriteMenuItem = new NativeMenuItem
            {
                Header = favorite.Title,
            };

            if (favorite.IsFolder)
            {
                if (favorite.Nodes.Count == 0)
                {
                    continue;
                }

                var menuItem = new NativeMenu();
                favoriteMenuItem.Menu = menuItem;

                AddFavoriteItems(menuItem.Items, favorite.Nodes);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(favorite.Path))
                {
                    continue;
                }

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

            if (parent is not null)
            {
                return parent;
            }
        }

        return null;
    }

    private static bool IsDescendantOf(IEnumerable<FavoriteItemViewModel> nodes, FavoriteItemViewModel item)
    {
        foreach (var node in nodes)
        {
            if (node == item)
            {
                return true;
            }

            if (IsDescendantOf(node.Nodes, item))
            {
                return true;
            }
        }

        return false;
    }

    private bool CanExecuteRemove => SelectedItem is not null && !SelectedItem.IsRoot;
    private bool CanExecuteInsert => SelectedItem is not null && (SelectedItem.IsFolder || SelectedItem.IsRoot);
    private bool CanExecuteCutCopy => SelectedItem is not null && !SelectedItem.IsRoot;
    private bool CanExecutePaste => SelectedItem is not null && (_cutItem is not null || _copyItem is not null);
    private bool CanMoveItemUp => SelectedItem is not null && !SelectedItem.IsRoot &&
                                  FindParent(Nodes, SelectedItem)?.Nodes.IndexOf(SelectedItem) > 0;
    private bool CanMoveItemDown => SelectedItem is not null && !SelectedItem.IsRoot &&
                                  FindParent(Nodes, SelectedItem)?.Nodes.IndexOf(SelectedItem) < FindParent(Nodes, SelectedItem)?.Nodes.Count - 1;
}
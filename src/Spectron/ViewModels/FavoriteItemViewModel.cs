using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public partial class FavoriteItemViewModel : ObservableObject
{
    public ObservableCollection<FavoriteItemViewModel>? Nodes { get; }

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private bool _isFolder;

    [ObservableProperty]
    private bool _isRoot;

    public FavoriteItemViewModel(string title)
    {
        Title = title;
        IsFolder = false;
    }

    public FavoriteItemViewModel(string title, ObservableCollection<FavoriteItemViewModel> nodes)
    {
        Title = title;
        Nodes = nodes;
        IsFolder = true;
    }
}
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public partial class FavoriteItemViewModel : ObservableObject
{
    public ObservableCollection<FavoriteItemViewModel> Nodes { get; } = [];

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private bool _isFolder;

    [ObservableProperty]
    private bool _isFile;

    [ObservableProperty]
    private bool _isRoot;
}
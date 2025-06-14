using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public partial class TapeBlockViewModel(int? index, string name, string data) : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public int? Index { get; init; } = index;
    public string Name { get; init; } = name;
    public string Data { get; init; } = data;
}
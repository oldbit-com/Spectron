using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public partial class TapeBlockViewModel(int? index, string name, string data) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }
    public int? Index { get; init; } = index;
    public string Name { get; init; } = name;
    public string Data { get; init; } = data;
}
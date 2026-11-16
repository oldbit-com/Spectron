using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public partial class GameViewModel : ObservableObject
{
    public int EntryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Availability { get; set; } = string.Empty;

    [ObservableProperty]
    public partial byte[] Data { get; set; }
}
using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public partial class TrainerItemPokeViewModel : ObservableObject
{
    [ObservableProperty]
    public partial byte? CustomValue { get; set; }

    public string Address { get; set; } = string.Empty;
}
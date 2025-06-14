using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.ViewModels;

public partial class TrainerItemPokeViewModel : ObservableObject
{
    [ObservableProperty]
    private byte? _customValue;

    public string Address { get; set; } = string.Empty;
}
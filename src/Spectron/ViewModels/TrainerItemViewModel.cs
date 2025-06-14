using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Files.Pok;

namespace OldBit.Spectron.ViewModels;

public partial class TrainerItemViewModel(Trainer trainer) : ObservableObject
{
    [ObservableProperty]
    private bool _isEnabled;

    public string Name { get; } = trainer.Name;
    public Trainer Trainer { get; } = trainer;
    public ObservableCollection<TrainerItemPokeViewModel> CustomPokes { get; } = [];
}
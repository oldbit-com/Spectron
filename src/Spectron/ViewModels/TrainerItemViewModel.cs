using System.Collections.ObjectModel;
using OldBit.Spectron.Files.Pok;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TrainerItemViewModel(Trainer trainer) : ReactiveObject
{
    private bool _isEnabled;

    public string Name { get; } = trainer.Name;

    public Trainer Trainer { get; } = trainer;

    public ObservableCollection<TrainerItemPokeViewModel> CustomPokes { get; } = [];

    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }
}
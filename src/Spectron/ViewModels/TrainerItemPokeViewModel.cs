using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TrainerItemPokeViewModel : ReactiveObject
{
    private byte? _customValue;

    public string Address { get; set; } = string.Empty;

    public byte? CustomValue
    {
        get => _customValue;
        set => this.RaiseAndSetIfChanged(ref _customValue, value);
    }
}
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TapeBlockViewModel(int? index, string name, string data) : ReactiveObject
{
    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public int? Index { get; init; } = index;
    public string Name { get; init; } = name;
    public string Data { get; init; } = data;
}
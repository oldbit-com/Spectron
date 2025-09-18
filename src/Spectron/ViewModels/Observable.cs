using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OldBit.Spectron.ViewModels;

public class Observable<T>(T value) : INotifyPropertyChanged
{
    private T _value = value;

    public T Value
    {
        get => _value;
        set
        {
            if (!Equals(value, _value))
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
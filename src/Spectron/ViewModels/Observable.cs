using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OldBit.Spectron.ViewModels;

public class Observable<T>(T value) : INotifyPropertyChanged
{
    public T Value
    {
        get;
        set
        {
            if (!Equals(value, field))
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = value;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
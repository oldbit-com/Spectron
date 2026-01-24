using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OldBit.Spectron.Debugger.ViewModels.Overlays;

public partial class BaseOverlayViewModel : ObservableValidator
{
    public Action Show { get; set; } = () => { };
    public Action Hide { get; set; } = () => { };

    [RelayCommand]
    public void OnHide() => Hide();

    [RelayCommand]
    public virtual void OnExecute() { }
}
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OldBit.Spectron.Debugger.ViewModels.Overlays;

public partial class FindOverlayViewModel : BaseOverlayViewModel
{
    [ObservableProperty]
    [Required(ErrorMessage = "Enter text to search for")]
    [NotifyDataErrorInfo]
    private string _text = "?";

    [RelayCommand]
    public void OnFind()
    {
        // if (!HexNumberParser.TryParse(Address, out var address))
        // {
        //     return;
        // }
        //
        // GoTo((Word)address);
        Hide();
    }
}
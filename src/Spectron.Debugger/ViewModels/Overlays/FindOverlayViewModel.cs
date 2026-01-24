using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OldBit.Spectron.Debugger.ViewModels.Overlays;

public partial class FindOverlayViewModel : BaseOverlayViewModel
{
    [ObservableProperty]
    [Required(ErrorMessage = "Enter text")]
    [NotifyDataErrorInfo]
    private string _text = "?";

    public override void OnExecute()
    {
        Hide();
    }
}
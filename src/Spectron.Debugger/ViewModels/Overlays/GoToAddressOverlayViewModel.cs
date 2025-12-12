using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace OldBit.Spectron.Debugger.ViewModels.Overlays;

public partial class GoToAddressOverlayViewModel : ObservableValidator
{
    [ObservableProperty]
    [Required(ErrorMessage = "Address is required.")]
    [CustomValidation(typeof(GoToAddressOverlayViewModel), nameof(ValidateAddress))]
    private string _address = "0";

    public Action Show { get; set; } = () => { };
    public Action Hide { get; set; } = () => { };
    public Action<Word> GoTo { get; set; } = _ => { };

    [RelayCommand]
    public void OnHide() => Hide();

    [RelayCommand]
    public void OnGoTo()
    {
        //
        Hide();
    }

    public static ValidationResult? ValidateAddress(string s, ValidationContext context)
    {
        return null;
    }
}
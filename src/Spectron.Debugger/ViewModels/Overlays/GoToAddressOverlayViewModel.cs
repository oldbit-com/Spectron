using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OldBit.Spectron.Debugger.Parser;

namespace OldBit.Spectron.Debugger.ViewModels.Overlays;

public partial class GoToAddressOverlayViewModel : ObservableValidator
{
    [ObservableProperty]
    [Required(ErrorMessage = "Address is required.")]
    [CustomValidation(typeof(GoToAddressOverlayViewModel), nameof(ValidateAddress))]
    [NotifyDataErrorInfo]
    private string _address = "0";

    public Action Show { get; set; } = () => { };
    public Action Hide { get; set; } = () => { };
    public Action<Word> GoTo { get; set; } = _ => { };

    [RelayCommand]
    public void OnHide() => Hide();

    [RelayCommand]
    public void OnGoTo()
    {
        if (!HexNumberParser.TryParse(Address, out var address))
        {
            return;
        }

        GoTo((Word)address);
        Hide();
    }

    public static ValidationResult? ValidateAddress(string s, ValidationContext context)
    {
        if (!HexNumberParser.TryParse(s, out var address))
        {
            return new ValidationResult("Invalid address format.");
        }

        if (address is < 0 or > 0xFFFF)
        {
            return new ValidationResult("Address out of range.");
        }

        return null;
    }
}
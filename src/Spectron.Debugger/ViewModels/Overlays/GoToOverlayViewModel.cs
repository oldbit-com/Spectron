using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Debugger.Parser;

namespace OldBit.Spectron.Debugger.ViewModels.Overlays;

public partial class GoToOverlayViewModel : BaseOverlayViewModel
{
    [ObservableProperty]
    [Required(ErrorMessage = "Enter address")]
    [CustomValidation(typeof(GoToOverlayViewModel), nameof(ValidateAddress))]
    [NotifyDataErrorInfo]
    private string _address = "0";

    public Action<Word> GoTo { get; set; } = _ => { };

    public override void OnExecute()
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
            return new ValidationResult("Invalid address.");
        }

        return address is < 0 or > 0xFFFF ? new ValidationResult("Address out of range.") : null;
    }
}
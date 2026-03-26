using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using OldBit.Spectron.Debugger.Breakpoints;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class BreakpointViewModel : ObservableValidator
{
    [ObservableProperty]
    [Required(ErrorMessage = "Condition is required.")]
    [CustomValidation(typeof(BreakpointViewModel), nameof(ValidateCondition))]
    public partial string Condition { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    private Breakpoint _breakpoint = null!;

    public BreakpointViewModel(Breakpoint breakpoint) => Breakpoint = breakpoint;

    public static ValidationResult? ValidateCondition(string? condition, ValidationContext context)
    {
        if (!BreakpointParser.TryParse(condition, out _))
        {
            return new ValidationResult("Invalid condition.", [nameof(Condition)]);
        }

        return ValidationResult.Success;
    }

    partial void OnIsEnabledChanged(bool value) => _breakpoint.IsEnabled = value;

    public Breakpoint Breakpoint
    {
        get => _breakpoint;
        set
        {
            _breakpoint = value;

            Condition = _breakpoint.Condition;
            IsEnabled = _breakpoint.IsEnabled;
        }
    }
}
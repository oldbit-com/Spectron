using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OldBit.Spectron.Debugger.Breakpoints;
using OldBit.Spectron.Debugger.Messages;

namespace OldBit.Spectron.Debugger.ViewModels;

public partial class BreakpointViewModel : ObservableValidator
{
    [ObservableProperty]
    [Required(ErrorMessage = "Condition is required.")]
    [CustomValidation(typeof(BreakpointViewModel), nameof(ValidateCondition))]
    private string _condition = string.Empty;

    [ObservableProperty]
    private bool _isEnabled;

    public BreakpointViewModel(Breakpoint breakpoint)
    {
        Breakpoint = breakpoint;
        Condition = breakpoint.ToString();
        IsEnabled = breakpoint.IsEnabled;
    }

    public static ValidationResult? ValidateCondition(string? condition, ValidationContext context)
    {
        if (!BreakpointParser.TryParseCondition(condition, out _))
        {
            return new ValidationResult("Invalid condition.", [nameof(Condition)]);
        }

        return ValidationResult.Success;
    }

    public Guid Id => Breakpoint.Id;

    public Breakpoint Breakpoint { get; }
}
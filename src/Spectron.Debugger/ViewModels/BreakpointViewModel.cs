using System.ComponentModel.DataAnnotations;
using OldBit.Spectron.Debugger.Breakpoints;
using ReactiveUI;

namespace OldBit.Spectron.Debugger.ViewModels;

public class BreakpointViewModel : ReactiveObject
{
    private string _condition = string.Empty;
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

    [Required(ErrorMessage = "Condition is required.")]
    [CustomValidation(typeof(BreakpointViewModel), nameof(ValidateCondition))]
    public string Condition
    {
        get => _condition;
        set => this.RaiseAndSetIfChanged(ref _condition, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
    }
}
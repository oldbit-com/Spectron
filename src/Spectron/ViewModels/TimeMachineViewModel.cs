using System.Collections.Generic;
using OldBit.Spectron.Emulation.TimeMachine;
using ReactiveUI;

namespace OldBit.Spectron.ViewModels;

public class TimeMachineViewModel : ViewModelBase
{
    private IReadOnlyList<TimeMachineState>? _snapshots;

    public void Update(IReadOnlyList<TimeMachineState>? snapshots)
    {
        _snapshots = snapshots;
        Maximum = _snapshots?.Count - 1 ?? 0;
        SelectedValue = Maximum;
    }

    private void UpdateScreen()
    {
        var index = (int)_selectedValue;
        var snapshot = _snapshots?[index];
    }

    private int _maximum = 1;
    public int Maximum
    {
        get => _maximum;
        set => this.RaiseAndSetIfChanged(ref _maximum, value);
    }

    private double _selectedValue;
    public double SelectedValue
    {
        get => _selectedValue;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedValue, value);
            UpdateScreen();
        }
    }
}
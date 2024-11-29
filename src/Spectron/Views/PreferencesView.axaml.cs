using ReactiveUI;
using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Input;
using OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

namespace OldBit.Spectron.Views;

public partial class PreferencesView : ReactiveWindow<PreferencesViewModel>
{
    public PreferencesView()
    {
        InitializeComponent();

        this.WhenActivated(action =>
            action(ViewModel!.ShowGamepadMappingView.RegisterHandler(ShowGamepadMappingViewAsync)));

        this.WhenActivated(action => action(ViewModel!.UpdatePreferencesCommand.Subscribe(Close)));
    }

    private async Task ShowGamepadMappingViewAsync(IInteractionContext<GamepadMappingViewModel, List<GamepadMapping>?> interaction)
    {
        var dialog = new GamepadMappingView() { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<List<GamepadMapping>?>(this);

        interaction.SetOutput(result);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }

        e.Handled = true;
        Close();
    }
}
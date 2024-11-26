using ReactiveUI;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using OldBit.Spectron.ViewModels;
using System;
using System.Threading.Tasks;
using Avalonia.Input;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Views;

public partial class PreferencesView : ReactiveWindow<PreferencesViewModel>
{
    public PreferencesView()
    {
        InitializeComponent();

        this.WhenActivated(action =>
            action(ViewModel!.ShowGamePadMappingView.RegisterHandler(ShowGamePadMappingViewAsync)));

        this.WhenActivated(action => action(ViewModel!.UpdatePreferencesCommand.Subscribe(Close)));
    }

    private async Task ShowGamePadMappingViewAsync(IInteractionContext<GamePadMappingViewModel, GamePadSettings?> interaction)
    {
        var dialog = new GamePadMappingView() { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<GamePadSettings?>(this);

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
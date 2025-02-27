using System;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Controls;

public partial class StatusBar : UserControl
{
    public StatusBar() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not StatusBarViewModel viewModel)
        {
            return;
        }

        if (Resources["QuickSaveAnimation"] is Animation animation)
        {
            viewModel.AnimateQuickSave = () =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    animation.RunAsync(QuickSaveIcon);
                });
            };
        }
    }
}
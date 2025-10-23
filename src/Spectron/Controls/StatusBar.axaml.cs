using System;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Threading;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Controls;

public partial class StatusBar : UserControl
{
    private Task _lastDiskActivityTask = Task.CompletedTask;

    public StatusBar() => InitializeComponent();

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (DataContext is not StatusBarViewModel viewModel)
        {
            return;
        }

        if (Resources["QuickSaveAnimation"] is Animation quickSaveAnimation)
        {
            viewModel.AnimateQuickSave = () =>
            {
                Dispatcher.UIThread.Post(() => quickSaveAnimation.RunAsync(QuickSaveIcon));
            };
        }

        if (Resources["DiskActivityAnimation"] is Animation diskActivityAnimation)
        {
            viewModel.AnimateDiskActivity = () =>
            {
                if (!_lastDiskActivityTask.IsCompleted)
                {
                    return;
                }

                _lastDiskActivityTask = Dispatcher.UIThread.InvokeAsync(async() =>
                {
                    await diskActivityAnimation.RunAsync(DiskActivityIcon);
                });
            };
        }
    }
}
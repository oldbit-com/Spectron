using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;

namespace OldBit.Spectron.Dialogs;

public class MessageDialogs : IMessageDialogs
{
    public async Task Error(string message, string title = "Error")
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            windowStartupLocation: WindowStartupLocation.CenterOwner);

        await ShowMessageDialog(messageBox);
    }

    public async Task Warning(string message, string title = "Warning")
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            windowStartupLocation: WindowStartupLocation.CenterOwner);

        await ShowMessageDialog(messageBox);
    }

    private static Window? GetActiveWindow()
    {
        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

        return lifetime?.Windows.FirstOrDefault(x => x.IsActive);
    }

    private static async Task ShowMessageDialog(IMsBox<ButtonResult> messageBox)
    {
        var activeWindow = GetActiveWindow();

        if (activeWindow != null)
        {
            await messageBox.ShowWindowDialogAsync(activeWindow);
        }
        else
        {
            await messageBox.ShowWindowAsync();
        }
    }
}
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;

namespace OldBit.Spectron.Dialogs;

public static class MessageDialogs
{
    public static Window? MainWindow { get; set; }

    public static async Task Error(string message, string title = "Error")
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            windowStartupLocation: WindowStartupLocation.CenterOwner);

        await messageBox.ShowWindowDialogAsync(MainWindow);
    }

    public static async Task Warning(string message, string title = "Warning")
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            windowStartupLocation: WindowStartupLocation.CenterOwner);

        await messageBox.ShowWindowDialogAsync(MainWindow);
    }
}
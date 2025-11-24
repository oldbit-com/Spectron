using Avalonia.Controls;

namespace OldBit.Spectron.Extensions;

public static class WindowExtensions
{
    public static void Open(this Window window, Window? owner)
    {
        if (owner != null)
        {
            window.Show(owner);
        }
        else
        {
            window.Show();
        }
    }
}
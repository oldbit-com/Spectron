using Avalonia;
using Avalonia.Styling;

namespace OldBit.Spectron.Theming;

public static class ThemeManager
{
    public static void SelectTheme(Theme theme)
    {
        var app = Application.Current;

        if (app == null)
        {
            return;
        }

        app.RequestedThemeVariant = theme switch
        {
            Theme.Dark => ThemeVariant.Dark,
            Theme.Light => ThemeVariant.Light,
            _ => app.RequestedThemeVariant
        };
    }
}
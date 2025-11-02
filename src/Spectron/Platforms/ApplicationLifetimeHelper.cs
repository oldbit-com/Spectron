using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using OldBit.Spectron.Platforms.Windows;

namespace OldBit.Spectron.Platforms;

/// <summary>
/// Provides a cross-platform helper class to manage application lifetime.
/// </summary>
public sealed class ApplicationLifetimeHelper : IDisposable
{
    private readonly WinActivateAppHook? _winActivateAppHook;

    public event EventHandler? AppActivated;
    public event EventHandler? AppDeactivated;

    public ApplicationLifetimeHelper(Application application, Window mainWindow)
    {
        var activatableLifetime = application?.TryGetFeature<IActivatableLifetime>();

        if (activatableLifetime != null)
        {
            activatableLifetime.Activated += (_, _) => AppActivated?.Invoke(this, EventArgs.Empty);
            activatableLifetime.Deactivated += (_, _) => AppDeactivated?.Invoke(this, EventArgs.Empty);
        }
        else if (OperatingSystem.IsWindows())
        {
            _winActivateAppHook = new WinActivateAppHook(mainWindow);

            _winActivateAppHook.AppActivated += (_, _) => AppActivated?.Invoke(this, EventArgs.Empty);
            _winActivateAppHook.AppDeactivated += (_, _) => AppDeactivated?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        if (OperatingSystem.IsWindows())
        {
            _winActivateAppHook?.Dispose();
        }
    }
}
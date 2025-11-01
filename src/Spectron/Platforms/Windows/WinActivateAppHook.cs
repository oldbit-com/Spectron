using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Controls;
using static OldBit.Spectron.Platforms.Windows.Interop.User32;

namespace OldBit.Spectron.Platforms.Windows;

/// <summary>
/// Represents a Windows-specific helper that monitors the activation and deactivation of an application window.
/// This class hooks into the Windows message loop for a given window to detect application activate and deactivate events.
/// This is a workaround for the Avalonia UI where it is not available.
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class WinActivateAppHook : IDisposable
{
    private IntPtr _hwnd;
    private IntPtr _oldWndProc;
    private WndProcDelegate? _wndProcDelegate; // root delegate to prevent GC

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    public event EventHandler? AppActivated;
    public event EventHandler? AppDeactivated;

    public WinActivateAppHook(Window window)
    {
        window.Opened += (_, _) =>
        {
            var handle = window.TryGetPlatformHandle();

            if (handle == null)
            {
                return;
            }

            if (!string.Equals(handle.HandleDescriptor, "HWND", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _hwnd = handle.Handle;
            _wndProcDelegate = WndProc;

            var newProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
            _oldWndProc = SetWindowLongPtr(_hwnd, GWLP_WNDPROC, newProcPtr);
        };

        window.Closed += (_, _) => Dispose();
    }

    private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_ACTIVATEAPP)
        {
            var isActivated = wParam != IntPtr.Zero;

            if (isActivated)
            {
                AppActivated?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                AppDeactivated?.Invoke(this, EventArgs.Empty);
            }
        }

        // Always call the original window proc
        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        if (_oldWndProc == IntPtr.Zero || _hwnd == IntPtr.Zero)
        {
            return;
        }

        SetWindowLongPtr(_hwnd, GWLP_WNDPROC, _oldWndProc);

        _oldWndProc = IntPtr.Zero;
        _hwnd = IntPtr.Zero;
    }
}
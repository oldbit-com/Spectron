using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OldBit.Spectron.Platforms.Windows.Interop;

[SupportedOSPlatform("windows")]
internal static partial class User32
{
    internal const int GWLP_WNDPROC = -4;
    internal const int WM_ACTIVATEAPP = 0x001C;

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    internal static partial IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [LibraryImport("user32.dll", EntryPoint = "CallWindowProcW")]
    internal static partial IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
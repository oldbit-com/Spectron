using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Mcp;

namespace OldBit.Spectron.ViewModels;

/// <summary>
/// Exposes the emulator to AI agents via the MCP server. All lifecycle operations are marshalled
/// to the UI thread, so the UI reflects what the agent does.
/// </summary>
partial class MainViewModel : IEmulatorHost
{
    private McpServer? _mcpServer;

    Emulator? IEmulatorHost.Emulator => Emulator;

    async Task IEmulatorHost.PauseAsync() =>
        await Dispatcher.UIThread.InvokeAsync(() => Pause(showOverlay: false));

    async Task IEmulatorHost.ResumeAsync() =>
        await Dispatcher.UIThread.InvokeAsync(() => Resume());

    async Task IEmulatorHost.ResetAsync(bool hardReset) =>
        await Dispatcher.UIThread.InvokeAsync(() => HandleMachineReset(hardReset));

    async Task IEmulatorHost.LoadFileAsync(string filePath) =>
        await Dispatcher.UIThread.InvokeAsync(() => HandleLoadFileAsync(filePath));

    private async Task ConfigureMcpServerAsync()
    {
        var settings = _preferences.McpServer;

        if (CommandLineArgs?.IsMcpServerEnabled == true)
        {
            settings = settings with
            {
                IsEnabled = true,
                Port = CommandLineArgs.McpServerPort ?? settings.Port
            };
        }

        if (!settings.IsEnabled)
        {
            await StopMcpServerAsync();
            return;
        }

        _mcpServer ??= new McpServer(this, _logger);

        if (_mcpServer.IsRunning && _mcpServer.Port == settings.Port)
        {
            return;
        }

        await _mcpServer.StopAsync();

        try
        {
            await _mcpServer.StartAsync(settings.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MCP server on port {Port}", settings.Port);

            NotificationManager.Show($"MCP server failed to start on port {settings.Port}: {ex.Message}",
                NotificationType.Error, TimeSpan.FromSeconds(5));
        }
    }

    private async Task StopMcpServerAsync()
    {
        if (_mcpServer != null)
        {
            await _mcpServer.StopAsync();
        }
    }
}

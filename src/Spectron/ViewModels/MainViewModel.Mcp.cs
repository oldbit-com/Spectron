using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Files;
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

    bool IEmulatorHost.IsPaused => IsPaused;

    async Task IEmulatorHost.PauseAsync() =>
        await Dispatcher.UIThread.InvokeAsync(() => Pause(showOverlay: false));

    async Task IEmulatorHost.ResumeAsync() =>
        await Dispatcher.UIThread.InvokeAsync(() => Resume());

    async Task IEmulatorHost.ResetAsync(bool hardReset) =>
        await Dispatcher.UIThread.InvokeAsync(() => HandleMachineReset(hardReset));

    async Task<FileType> IEmulatorHost.LoadFileAsync(string filePath, string? archiveEntry) =>
        await Dispatcher.UIThread.InvokeAsync(() => LoadFileForAgentAsync(filePath, archiveEntry));

    /// <summary>
    /// Loads a file like File/Open does, but throws instead of showing error dialogs or the archive file picker,
    /// so the agent's tool call can't wait for the user.
    /// </summary>
    private async Task<FileType> LoadFileForAgentAsync(string filePath, string? archiveEntry)
    {
        var shouldResume = !IsPaused;

        try
        {
            Pause(showOverlay: false);

            var fileType = await LoadFileCoreAsync(filePath, FileTypeResolver.FromPath(filePath), favorite: null,
                entries => Task.FromResult<ArchiveEntry?>(FindArchiveEntry(entries, archiveEntry)));

            return fileType ?? throw new InvalidOperationException("Nothing was loaded.");
        }
        finally
        {
            if (shouldResume)
            {
                Resume();
            }
        }
    }

    private static ArchiveEntry FindArchiveEntry(List<ArchiveEntry> entries, string? name)
    {
        if (name == null)
        {
            return entries.Count == 1
                ? entries[0]
                : throw new InvalidOperationException($"The archive contains {entries.Count} supported files, choose one.");
        }

        return entries.FirstOrDefault(entry => entry.Name == name) ??
               throw new InvalidOperationException($"File {name} not found in the archive.");
    }

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

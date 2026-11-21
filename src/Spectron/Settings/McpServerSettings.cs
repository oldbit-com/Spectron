using OldBit.Spectron.Mcp;

namespace OldBit.Spectron.Settings;

public record McpServerSettings
{
    public bool IsEnabled { get; init; }

    public int Port { get; init; } = McpServer.DefaultPort;
}

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Mcp.Tools;

namespace OldBit.Spectron.Mcp;

/// <summary>
/// Hosts the MCP server over Streamable HTTP, bound to the loopback interface only.
/// </summary>
public sealed class McpServer(IEmulatorHost host, ILogger logger) : IAsyncDisposable
{
    public const int DefaultPort = 5480;
    public const string EndpointPath = "/mcp";

    private WebApplication? _app;

    public bool IsRunning => _app != null;

    public int Port { get; private set; }

    public string Url => $"http://localhost:{Port}{EndpointPath}";

    public async Task StartAsync(int port = DefaultPort)
    {
        if (_app != null)
        {
            return;
        }

        var builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port));
        builder.Logging.ClearProviders();

        builder.Services.AddSingleton(host);
        builder.Services.AddSingleton<EmulatorController>();
        builder.Services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new() { Name = "Spectron", Version = typeof(McpServer).Assembly.GetName().Version?.ToString() ?? "0.0.0" };
                options.ServerInstructions =
                    """
                    Controls a running ZX Spectrum emulator. Typical loop: load_file or type_text, run_frames to let time pass,
                    then read_screen_text (cheap) or screenshot (when graphics matter). Emulation runs in real time until
                    pause or run_frames is used; run_frames leaves it paused, so call resume to hand control back to the user.
                    """;
            })
            .WithHttpTransport(options => options.Stateless = true)
            .WithTools<EmulatorTools>();

        var app = builder.Build();
        app.MapMcp(EndpointPath);

        await app.StartAsync();

        _app = app;
        Port = port;

        logger.LogInformation("MCP server listening on {Url}", Url);
    }

    public async Task StopAsync()
    {
        if (_app == null)
        {
            return;
        }

        var app = _app;
        _app = null;

        await app.StopAsync();
        await app.DisposeAsync();

        logger.LogInformation("MCP server stopped");
    }

    public async ValueTask DisposeAsync() => await StopAsync();
}

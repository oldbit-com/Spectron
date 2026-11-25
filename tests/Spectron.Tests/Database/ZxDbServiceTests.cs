using System.Net;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using OldBit.Spectron.Database;
using OldBit.Spectron.Database.Data;

namespace OldBit.Spectron.Tests.Database;

public sealed class ZxDbServiceTests : IDisposable
{
    private const int EntryId = 100;

    private readonly ZxDbTestDatabase _database = ZxDbTestDatabase.Create();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly List<Uri> _requests = [];
    private readonly string _downloadDirectory = Path.Join(Path.GetTempPath(), $"spectron-zxdb-{Guid.NewGuid():N}");
    private readonly ZxDbService _service;

    private HttpStatusCode _statusCode = HttpStatusCode.OK;

    public ZxDbServiceTests()
    {
        var handler = new TestHttpMessageHandler(request =>
        {
            _requests.Add(request.RequestUri!);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new ByteArrayContent([1, 2, 3])
            };
        });

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));

        var settings = new ZxDbSettings { DatabasePath = _database.Path, DownloadDirectory = _downloadDirectory };
        var client = new ZxDbClient(new ZxDbContextFactory(settings), settings);

        _service = new ZxDbService(client, settings, _cache, factory);
    }

    [Fact]
    public async Task GetScreenshot_ShouldDownloadResolvedUrl()
    {
        _database.AddDownload(1, EntryId, "/pub/sinclair/screens/load/g/scr/Game.scr", fileTypeId: 1);

        var screenshot = await _service.GetScreenshotAsync(EntryId, TestContext.Current.CancellationToken);

        screenshot.ShouldBe([1, 2, 3]);
        _requests.ShouldHaveSingleItem().ToString()
            .ShouldBe("https://worldofspectrum.net/pub/sinclair/screens/load/g/scr/Game.scr");
    }

    [Fact]
    public async Task GetScreenshot_ShouldUseCache_OnSubsequentCalls()
    {
        _database.AddDownload(1, EntryId, "/pub/sinclair/screens/load/g/scr/Game.scr", fileTypeId: 1);

        await _service.GetScreenshotAsync(EntryId, TestContext.Current.CancellationToken);
        var screenshot = await _service.GetScreenshotAsync(EntryId, TestContext.Current.CancellationToken);

        screenshot.ShouldBe([1, 2, 3]);
        _requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetScreenshot_ShouldNotCollide_WithOtherCacheEntries()
    {
        _database.AddDownload(1, EntryId, "/pub/sinclair/screens/load/g/scr/Game.scr", fileTypeId: 1);
        _cache.Set(EntryId, new byte[] { 9 });

        var screenshot = await _service.GetScreenshotAsync(EntryId, TestContext.Current.CancellationToken);

        screenshot.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task GetScreenshot_ShouldReturnNull_WhenNoScreenshot()
    {
        var screenshot = await _service.GetScreenshotAsync(EntryId, TestContext.Current.CancellationToken);

        screenshot.ShouldBeNull();
        _requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task DownloadFile_ShouldSaveFile_MirroringRemotePath()
    {
        var filePath = await _service.DownloadFileAsync("/pub/sinclair/games/j/Jet Set Willy.tzx.zip", TestContext.Current.CancellationToken);

        filePath.ShouldBe(Path.Join(_downloadDirectory, "worldofspectrum.net", "pub", "sinclair", "games", "j", "Jet Set Willy.tzx.zip"));
        (await File.ReadAllBytesAsync(filePath!, TestContext.Current.CancellationToken)).ShouldBe([1, 2, 3]);
        _requests.ShouldHaveSingleItem().ToString()
            .ShouldBe("https://worldofspectrum.net/pub/sinclair/games/j/Jet Set Willy.tzx.zip");
    }

    [Fact]
    public async Task DownloadFile_ShouldUseLocalCopy_OnSubsequentCalls()
    {
        var first = await _service.DownloadFileAsync("/zxdb/sinclair/entries/0002259/Game.tap", TestContext.Current.CancellationToken);
        var second = await _service.DownloadFileAsync("/zxdb/sinclair/entries/0002259/Game.tap", TestContext.Current.CancellationToken);

        second.ShouldBe(first);
        _requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task DownloadFile_ShouldReturnNull_WhenLinkCannotBeResolved()
    {
        var filePath = await _service.DownloadFileAsync("unknown/Game.tap", TestContext.Current.CancellationToken);

        filePath.ShouldBeNull();
        _requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task DownloadFile_ShouldNotLeaveFiles_WhenDownloadFails()
    {
        _statusCode = HttpStatusCode.NotFound;

        await Should.ThrowAsync<HttpRequestException>(() =>
            _service.DownloadFileAsync("/pub/sinclair/games/g/Game.tap", TestContext.Current.CancellationToken));

        Directory.EnumerateFiles(_downloadDirectory, "*", SearchOption.AllDirectories).ShouldBeEmpty();
    }

    [Fact]
    public async Task DownloadFile_ShouldKeepFilesInsideDownloadDirectory()
    {
        var filePath = await _service.DownloadFileAsync("/pub/sinclair/games/%2E%2E/%2E%2E/Game.tap", TestContext.Current.CancellationToken);

        Path.GetFullPath(filePath!).ShouldStartWith(Path.GetFullPath(_downloadDirectory) + Path.DirectorySeparatorChar);
    }

    public void Dispose()
    {
        if (Directory.Exists(_downloadDirectory))
        {
            Directory.Delete(_downloadDirectory, recursive: true);
        }

        _cache.Dispose();
        _database.Dispose();
    }
}

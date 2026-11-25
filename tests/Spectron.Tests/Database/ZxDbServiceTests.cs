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
    private readonly ZxDbService _service;

    public ZxDbServiceTests()
    {
        var handler = new TestHttpMessageHandler(request =>
        {
            _requests.Add(request.RequestUri!);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent([1, 2, 3])
            };
        });

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));

        var settings = new ZxDbSettings { DatabasePath = _database.Path };
        var client = new ZxDbClient(new ZxDbContextFactory(settings), settings);

        _service = new ZxDbService(client, _cache, factory);
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

    public void Dispose()
    {
        _cache.Dispose();
        _database.Dispose();
    }
}

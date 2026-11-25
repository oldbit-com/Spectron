using OldBit.Spectron.Database;
using OldBit.Spectron.Database.Data;

namespace OldBit.Spectron.Tests.Database;

public sealed class ZxDbClientTests : IDisposable
{
    private const int EntryId = 100;

    private readonly ZxDbTestDatabase _database = ZxDbTestDatabase.Create();
    private readonly ZxDbClient _client;

    public ZxDbClientTests()
    {
        var settings = new ZxDbSettings { DatabasePath = _database.Path };
        _client = new ZxDbClient(new ZxDbContextFactory(settings), settings);
    }

    [Fact]
    public async Task GetScreenshotFile_ShouldReturnNull_WhenNoScreenshot()
    {
        _database.AddDownload(1, EntryId, "/pub/sinclair/games/g/Game.tzx.zip", fileTypeId: 8);

        var file = await _client.GetScreenshotFileAsync(EntryId, TestContext.Current.CancellationToken);

        file.ShouldBeNull();
    }

    [Fact]
    public async Task GetScreenshotFile_ShouldReturnFirstScrFile()
    {
        _database.AddDownload(3, EntryId, "/pub/sinclair/screens/load/g/scr/Game2.scr", fileTypeId: 1);
        _database.AddDownload(2, EntryId, "/pub/sinclair/screens/load/g/scr/Game1.SCR", fileTypeId: 1);
        _database.AddDownload(4, EntryId + 1, "/pub/sinclair/screens/load/o/scr/Other.scr", fileTypeId: 1);

        var file = await _client.GetScreenshotFileAsync(EntryId, TestContext.Current.CancellationToken);

        file.ShouldBe("/pub/sinclair/screens/load/g/scr/Game1.SCR");
    }

    [Fact]
    public async Task GetScreenshotFile_ShouldIgnoreNonScrFiles()
    {
        _database.AddDownload(1, EntryId, "/pub/sinclair/screens/load/g/gif/Game.gif", fileTypeId: 1);
        _database.AddDownload(2, EntryId, "/zxdb/sinclair/entries/0000100/Game.png", fileTypeId: 1);

        var file = await _client.GetScreenshotFileAsync(EntryId, TestContext.Current.CancellationToken);

        file.ShouldBeNull();
    }

    [Fact]
    public async Task GetGameFileLinks_ShouldReturnLoadableFiles_InReleaseOrder()
    {
        _database.AddDownload(1, EntryId, "/pub/sinclair/screens/load/g/scr/Game.scr", fileTypeId: 1);
        _database.AddDownload(2, EntryId, "/pub/sinclair/games/g/Game (Re-release).tzx.zip", fileTypeId: 8, releaseSeq: 1);
        _database.AddDownload(3, EntryId, "/pub/sinclair/games/g/Game.z80.zip", fileTypeId: 10);
        _database.AddDownload(4, EntryId, "/pub/sinclair/games/g/Game.tzx.zip", fileTypeId: 8);
        _database.AddDownload(5, EntryId, "/pub/sinclair/games/g/Game.dsk.zip", fileTypeId: 11);
        _database.AddDownload(6, EntryId, "/pub/sinclair/games/g/Game (Bugfix).tap.zip", fileTypeId: 22);
        _database.AddDownload(7, EntryId + 1, "/pub/sinclair/games/o/Other.tzx.zip", fileTypeId: 8);

        var files = await _client.GetGameFileLinksAsync(EntryId, TestContext.Current.CancellationToken);

        files.Select(f => f.FileLink).ShouldBe([
            "/pub/sinclair/games/g/Game.z80.zip",
            "/pub/sinclair/games/g/Game.tzx.zip",
            "/pub/sinclair/games/g/Game.dsk.zip",
            "/pub/sinclair/games/g/Game (Bugfix).tap.zip",
            "/pub/sinclair/games/g/Game (Re-release).tzx.zip"
        ]);
    }

    public void Dispose() => _database.Dispose();
}

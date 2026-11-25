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
        _database.AddDownload(8, EntryId, "/zxdb/sinclair/entries/0000100/Game.rzx.zip", fileTypeId: 63);
        _database.AddDownload(9, EntryId, "/pub/sinclair/pokes/g/Game.pok", fileTypeId: 74);

        var files = await _client.GetGameFileLinksAsync(EntryId, TestContext.Current.CancellationToken);

        files.Select(f => f.FileLink).ShouldBe([
            "/pub/sinclair/games/g/Game.z80.zip",
            "/pub/sinclair/games/g/Game.tzx.zip",
            "/pub/sinclair/games/g/Game (Bugfix).tap.zip",
            "/zxdb/sinclair/entries/0000100/Game.rzx.zip",
            "/pub/sinclair/pokes/g/Game.pok",
            "/pub/sinclair/games/g/Game (Re-release).tzx.zip"
        ]);
    }

    [Fact]
    public async Task Search_ShouldReturnEmpty_WhenTextHasNoLettersOrDigits()
    {
        _database.AddEntry(1, "Manic Miner", 1983);

        var results = await _client.SearchAsync(" :- ", GameSearchField.Title, 10, TestContext.Current.CancellationToken);

        results.ShouldBeEmpty();
    }

    [Fact]
    public async Task Search_ByTitle_ShouldMatchNormalizedTitle_OrderedByTitle()
    {
        _database.AddEntry(1, "Manic Miner", 1983);
        _database.AddEntry(2, "Jet Set Willy 64: Manic Miner", 1985);
        _database.AddEntry(3, "Jet Set Willy", 1984);
        _database.AddLabel(10, "Bug-Byte Software Ltd");
        _database.AddPublisher(1, 10);

        var results = await _client.SearchAsync("manic-miner", GameSearchField.Title, 10, TestContext.Current.CancellationToken);

        results.Select(r => r.Title).ShouldBe(["Jet Set Willy 64: Manic Miner", "Manic Miner"]);
        results[1].Publisher.ShouldBe("Bug-Byte Software Ltd");
        results[1].Year.ShouldBe((short)1983);
    }

    [Fact]
    public async Task Search_ByPublisher_ShouldMatchAnyMatchingLabel_OrderedByYear()
    {
        _database.AddEntry(1, "Head over Heels", 1987);
        _database.AddEntry(2, "Batman", 1986);
        _database.AddEntry(3, "Unknown Year Game");
        _database.AddEntry(4, "Daley Thompson's Decathlon", 1984);
        _database.AddEntry(5, "Sabre Wulf", 1984);
        _database.AddLabel(10, "Ocean Software Ltd");
        _database.AddLabel(11, "Ocean Software France");
        _database.AddLabel(12, "Ultimate Play The Game");
        _database.AddPublisher(1, 10);
        _database.AddPublisher(2, 11);
        _database.AddPublisher(3, 10);
        _database.AddPublisher(4, 10);
        _database.AddPublisher(5, 12);

        var results = await _client.SearchAsync("Ocean", GameSearchField.Publisher, 10, TestContext.Current.CancellationToken);

        results.Select(r => r.Title).ShouldBe([
            "Daley Thompson's Decathlon",
            "Batman",
            "Head over Heels",
            "Unknown Year Game"
        ]);
    }

    [Fact]
    public async Task Search_ByPublisher_ShouldIncludeReReleases()
    {
        _database.AddEntry(1, "Knight Lore", 1984);
        _database.AddLabel(10, "Ultimate Play The Game");
        _database.AddLabel(11, "The Hit Squad");
        _database.AddPublisher(1, 10);
        _database.AddPublisher(1, 11, releaseSeq: 1);

        var results = await _client.SearchAsync("hit squad", GameSearchField.Publisher, 10, TestContext.Current.CancellationToken);

        results.ShouldHaveSingleItem().Title.ShouldBe("Knight Lore");
        results[0].Publisher.ShouldBe("Ultimate Play The Game");
    }

    [Fact]
    public async Task Search_ByAuthor_ShouldMatchAuthorLabels()
    {
        _database.AddEntry(1, "Manic Miner", 1983);
        _database.AddEntry(2, "Jet Set Willy", 1984);
        _database.AddEntry(3, "Skool Daze", 1984);
        _database.AddLabel(20, "Matthew Smith");
        _database.AddLabel(21, "David S. Reidy");
        _database.AddAuthor(1, 20);
        _database.AddAuthor(2, 20);
        _database.AddAuthor(3, 21);

        var results = await _client.SearchAsync("matthew smith", GameSearchField.Author, 10, TestContext.Current.CancellationToken);

        results.Select(r => r.Title).ShouldBe(["Manic Miner", "Jet Set Willy"]);
    }

    [Fact]
    public async Task Search_ShouldExcludeNonSpectrumMachines_AndApplyLimit()
    {
        _database.AddEntry(1, "Game A", 1984, machineTypeId: 1);
        _database.AddEntry(2, "Game B", 1985, machineTypeId: 12);
        _database.AddEntry(3, "Game C", 1986, machineTypeId: 13);
        _database.AddEntry(4, "Game D", 1987);

        var results = await _client.SearchAsync("game", GameSearchField.Title, 2, TestContext.Current.CancellationToken);

        results.Select(r => r.Title).ShouldBe(["Game A", "Game C"]);
    }

    public void Dispose() => _database.Dispose();
}

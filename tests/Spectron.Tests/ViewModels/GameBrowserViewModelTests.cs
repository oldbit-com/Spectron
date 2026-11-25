using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OldBit.Spectron.Database;
using OldBit.Spectron.Database.Data;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;
using OldBit.Spectron.Tests.Database;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class GameBrowserViewModelTests : IDisposable
{
    private readonly ZxDbSettings _settings = new();
    private readonly IApplicationDataService _applicationDataService = Substitute.For<IApplicationDataService>();
    private readonly GameBrowserViewModel _viewModel;
    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public GameBrowserViewModelTests()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([])
        });

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));

        var client = new ZxDbClient(new ZxDbContextFactory(_settings), _settings);
        var service = new ZxDbService(client, _settings, _cache, factory);
        var fileDialogs = new FileDialogs(Substitute.For<IFileDialogProvider>());
        var logger = Substitute.For<ILogger<GameBrowserViewModel>>();

        _viewModel = new GameBrowserViewModel(service, _settings, _applicationDataService, logger, fileDialogs,
            Substitute.For<IMessageDialogs>());
    }

    [Fact]
    public async Task Initialize_ShouldBeUnavailable_WhenPathNotSaved()
    {
        _applicationDataService.LoadAsync<GameBrowserSettings>().Returns(new GameBrowserSettings());

        await _viewModel.InitializeAsync();

        _viewModel.IsDatabaseAvailable.ShouldBeFalse();
        _settings.DatabasePath.ShouldBeNull();
    }

    [Fact]
    public async Task Initialize_ShouldApplySavedPath()
    {
        using var database = ZxDbTestDatabase.Create();
        _applicationDataService.LoadAsync<GameBrowserSettings>().Returns(new GameBrowserSettings { ZxDbPath = database.Path });

        await _viewModel.InitializeAsync();

        _viewModel.IsDatabaseAvailable.ShouldBeTrue();
        _settings.DatabasePath.ShouldBe(database.Path);
    }

    [Fact]
    public async Task Initialize_ShouldApplySavedSearchField()
    {
        _applicationDataService.LoadAsync<GameBrowserSettings>()
            .Returns(new GameBrowserSettings { SearchField = GameSearchField.Author });

        await _viewModel.InitializeAsync();

        _viewModel.SearchField.ShouldBe(GameSearchField.Author);
        _viewModel.SearchPlaceholder.ShouldBe("Search games by author");
        await _applicationDataService.DidNotReceive().SaveAsync(Arg.Any<object>());
    }

    [Fact]
    public async Task SearchField_ShouldSaveSettings_WhenChanged()
    {
        using var database = ZxDbTestDatabase.Create();
        _applicationDataService.LoadAsync<GameBrowserSettings>().Returns(new GameBrowserSettings { ZxDbPath = database.Path });
        await _viewModel.InitializeAsync();

        _viewModel.SearchField = GameSearchField.Publisher;

        await _applicationDataService.Received(1).SaveAsync(Arg.Is<GameBrowserSettings>(s =>
            s.ZxDbPath == database.Path && s.SearchField == GameSearchField.Publisher));
    }

    [Fact]
    public async Task SearchField_ShouldRerunSearch_WhenChanged()
    {
        using var database = ZxDbTestDatabase.Create();
        database.AddEntry(1, "Batman", 1986);
        database.AddEntry(2, "Ocean Conqueror", 1987);
        database.AddLabel(10, "Ocean Software Ltd");
        database.AddPublisher(1, 10);

        _applicationDataService.LoadAsync<GameBrowserSettings>().Returns(new GameBrowserSettings { ZxDbPath = database.Path });
        await _viewModel.InitializeAsync();

        _viewModel.SearchText = "ocean";
        await _viewModel.SearchCommand.ExecuteAsync(null);
        _viewModel.Games.Select(g => g.Name).ShouldBe(["Ocean Conqueror"]);

        _viewModel.SearchField = GameSearchField.Publisher;
        await _viewModel.SearchCommand.ExecutionTask!;

        _viewModel.Games.Select(g => g.Name).ShouldBe(["Batman"]);
    }

    [Theory]
    [InlineData(0, "No games found")]
    [InlineData(1, "1 game found")]
    [InlineData(3, "3 games found")]
    public async Task Search_ShouldSummarizeResultCount(int count, string expectedSummary)
    {
        using var database = ZxDbTestDatabase.Create();

        for (var i = 1; i <= count; i++)
        {
            database.AddEntry(i, $"Game {i}", 1984);
        }

        _applicationDataService.LoadAsync<GameBrowserSettings>().Returns(new GameBrowserSettings { ZxDbPath = database.Path });
        await _viewModel.InitializeAsync();

        _viewModel.SearchText = "game";
        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Games.Count.ShouldBe(count);
        _viewModel.SearchSummary.ShouldBe(expectedSummary);
    }

    [Fact]
    public async Task Search_ShouldReportTruncation_WhenMoreMatchesThanLimit()
    {
        using var database = ZxDbTestDatabase.Create();

        for (var i = 1; i <= ZxDbService.DefaultSearchLimit + 1; i++)
        {
            database.AddEntry(i, $"Game {i}", 1984);
        }

        _applicationDataService.LoadAsync<GameBrowserSettings>().Returns(new GameBrowserSettings { ZxDbPath = database.Path });
        await _viewModel.InitializeAsync();

        _viewModel.SearchText = "game";
        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Games.Count.ShouldBe(ZxDbService.DefaultSearchLimit);
        _viewModel.SearchSummary.ShouldBe(
            $"Showing first {ZxDbService.DefaultSearchLimit} matches, refine your search to see more");
    }

    [Fact]
    public async Task SetDatabasePath_ShouldSaveAndApplyPath_WhenValid()
    {
        using var database = ZxDbTestDatabase.Create();

        await _viewModel.SetDatabasePathAsync(database.Path);

        _viewModel.IsDatabaseAvailable.ShouldBeTrue();
        _viewModel.DatabaseError.ShouldBeNull();
        _settings.DatabasePath.ShouldBe(database.Path);
        await _applicationDataService.Received(1).SaveAsync(Arg.Is<GameBrowserSettings>(s => s.ZxDbPath == database.Path));
    }

    [Fact]
    public async Task SetDatabasePath_ShouldReportError_WhenInvalid()
    {
        using var database = ZxDbTestDatabase.Create(isValidSchema: false);

        await _viewModel.SetDatabasePathAsync(database.Path);

        _viewModel.IsDatabaseAvailable.ShouldBeFalse();
        _viewModel.DatabaseError.ShouldNotBeNull();
        _settings.DatabasePath.ShouldBeNull();
        await _applicationDataService.DidNotReceive().SaveAsync(Arg.Any<object>());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _cache.Dispose();
    }
}

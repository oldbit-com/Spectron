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

public class GameBrowserViewModelTests
{
    private readonly ZxDbSettings _settings = new();
    private readonly IApplicationDataService _applicationDataService = Substitute.For<IApplicationDataService>();
    private readonly GameBrowserViewModel _viewModel;

    public GameBrowserViewModelTests()
    {
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent([])
        });

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(handler, disposeHandler: false));

        var cache = new MemoryCache(new MemoryCacheOptions());
        var client = new ZxDbClient(new ZxDbContextFactory(_settings), _settings);
        var service = new ZxDbService(client, _settings, cache, factory);
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
}

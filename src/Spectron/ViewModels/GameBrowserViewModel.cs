using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Database;
using OldBit.Spectron.Database.Results;
using OldBit.Spectron.Dialogs;
using OldBit.Spectron.Messages;
using OldBit.Spectron.Services;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.ViewModels;

public partial class GameBrowserViewModel : ObservableObject
{
    private readonly ZxDbService _zxDbService;
    private readonly ZxDbSettings _zxDbSettings;
    private readonly IApplicationDataService _applicationDataService;
    private readonly ILogger<GameBrowserViewModel> _logger;
    private readonly FileDialogs _fileDialogs;
    private readonly IMessageDialogs _messageDialogs;
    private readonly byte[] _emptyScreenshot;

    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromSeconds(30);

    private bool _isInitialized;

    public ObservableCollection<GameViewModel> Games { get; } = [];

    public IAsyncRelayCommand SearchCommand { get; }

    public IAsyncRelayCommand LocateDatabaseCommand { get; }

    public IAsyncRelayCommand<TopLevel?> DownloadFileCommand { get; }

    public IReadOnlyList<NameValuePair<GameSearchField>> SearchFields { get; } =
    [
        new("Title", GameSearchField.Title),
        new("Publisher", GameSearchField.Publisher),
        new("Author", GameSearchField.Author)
    ];

    [ObservableProperty]
    public partial GameViewModel? SelectedGame { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchPlaceholder))]
    public partial GameSearchField SearchField { get; set; } = GameSearchField.Title;

    public string SearchPlaceholder => SearchField switch
    {
        GameSearchField.Publisher => "Search games by publisher",
        GameSearchField.Author => "Search games by author",
        _ => "Search games by title"
    };

    [ObservableProperty]
    public partial bool IsDatabaseAvailable { get; private set; }

    [ObservableProperty]
    public partial string? DatabaseError { get; private set; }

    [ObservableProperty]
    public partial string? SearchSummary { get; private set; }

    public GameBrowserViewModel(
        ZxDbService zxDbService,
        ZxDbSettings zxDbSettings,
        IApplicationDataService applicationDataService,
        ILogger<GameBrowserViewModel> logger,
        FileDialogs fileDialogs,
        IMessageDialogs messageDialogs)
    {
        _zxDbService = zxDbService;
        _zxDbSettings = zxDbSettings;
        _applicationDataService = applicationDataService;
        _logger = logger;
        _fileDialogs = fileDialogs;
        _messageDialogs = messageDialogs;

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("OldBit.Spectron.Resources.Spectrum.scr")!;
        using var reader = new BinaryReader(stream);
        _emptyScreenshot = reader.ReadBytes((int)stream.Length);

        SearchCommand = new AsyncRelayCommand(SearchAsync);
        LocateDatabaseCommand = new AsyncRelayCommand(LocateDatabaseAsync);
        DownloadFileCommand = new AsyncRelayCommand<TopLevel?>(DownloadFileAsync);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            RefreshAvailability();
            return;
        }

        var settings = await _applicationDataService.LoadAsync<GameBrowserSettings>();

        _zxDbSettings.DatabasePath = settings.ZxDbPath;
        SearchField = settings.SearchField;
        _isInitialized = true;

        RefreshAvailability();
    }

    partial void OnSelectedGameChanged(GameViewModel? value) => _ = LoadEntryDataAsync(value);

    partial void OnSearchFieldChanged(GameSearchField value)
    {
        if (!_isInitialized)
        {
            return;
        }

        _ = SaveSettingsAsync();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            _ = SearchCommand.ExecuteAsync(null);
        }
    }

    internal async Task SetDatabasePathAsync(string databasePath)
    {
        if (!await ZxDbService.IsValidDatabaseAsync(databasePath))
        {
            DatabaseError = "The selected file is not a valid ZXDB database.";
            return;
        }

        _zxDbSettings.DatabasePath = databasePath;
        await SaveSettingsAsync();

        Games.Clear();
        DatabaseError = null;
        RefreshAvailability();
    }

    private Task SaveSettingsAsync() => _applicationDataService.SaveAsync(new GameBrowserSettings
    {
        ZxDbPath = _zxDbSettings.DatabasePath,
        SearchField = SearchField
    });

    private async Task LocateDatabaseAsync()
    {
        var files = await _fileDialogs.OpenZxDbFileAsync();

        if (files.Count == 0)
        {
            return;
        }

        var databasePath = files[0].TryGetLocalPath();

        if (databasePath == null)
        {
            DatabaseError = "The selected file is not a local file.";
            return;
        }

        await SetDatabasePathAsync(databasePath);
    }

    private async Task DownloadFileAsync(TopLevel? topLevel)
    {
        if (topLevel is null || SelectedGame?.SelectedFile is null)
        {
            return;
        }

        var url = ZxDbFileUrlResolver.Resolve(SelectedGame.SelectedFile);

        if (url is null)
        {
            return;
        }

        await topLevel.Launcher.LaunchUriAsync(new Uri(url));
    }

    [RelayCommand]
    private async Task LoadFileAsync(CancellationToken cancellationToken)
    {
        var fileLink = SelectedGame?.SelectedFile;

        if (fileLink is null)
        {
            return;
        }

        string? filePath;

        try
        {
            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tokenSource.CancelAfter(DownloadTimeout);

            filePath = await _zxDbService.DownloadFileAsync(fileLink, tokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileLink}", fileLink);
            await _messageDialogs.Error($"Unable to download {Path.GetFileName(fileLink)}: {ex.Message}");

            return;
        }

        if (filePath is null)
        {
            await _messageDialogs.Warning($"Unable to resolve download location for {Path.GetFileName(fileLink)}.");
            return;
        }

        WeakReferenceMessenger.Default.Send(new LoadFileMessage(filePath));
    }

    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return;
        }

        RefreshAvailability();

        if (!IsDatabaseAvailable)
        {
            return;
        }

        try
        {
            var results = await _zxDbService.SearchAsync(SearchText, SearchField, cancellationToken: cancellationToken);

            Games.Clear();

            foreach (var result in results.Games)
            {
                Games.Add(new GameViewModel
                {
                    EntryId = result.Id,
                    Name = result.Title,
                    Publisher = result.Publisher ?? "Unknown",
                    Year = result.Year?.ToString() ?? "????",
                    Genre = result.Genre ?? string.Empty,
                    Availability = result.Availability ?? string.Empty,
                    Machine = result.Machine ?? string.Empty
                });
            }

            SearchSummary = GetSearchSummary(results);
        }
        catch (Exception ex) when (ex is SqliteException or ZxDbNotConfiguredException)
        {
            DatabaseError = $"Unable to read ZXDB database: {ex.Message}";
            RefreshAvailability();
        }
    }

    private static string GetSearchSummary(GameSearchResults results) => results switch
    {
        { IsTruncated: true } => $"Showing first {results.Games.Count} matches, refine your search to see more",
        { Games.Count: 0 } => "No games found",
        { Games.Count: 1 } => "1 game found",
        _ => $"{results.Games.Count} games found"
    };

    private void RefreshAvailability() => IsDatabaseAvailable = _zxDbService.IsAvailable;

    private async Task LoadEntryDataAsync(GameViewModel? game)
    {
        if (game == null)
        {
            return;
        }

        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await Task.WhenAll(
            LoadScreenshotAsync(game, tokenSource.Token),
            LoadFilesAsync(game, tokenSource.Token));
    }

    private async Task LoadScreenshotAsync(GameViewModel game, CancellationToken cancellationToken)
    {
        try
        {
            var screenshot = await _zxDbService.GetScreenshotAsync(game.EntryId, cancellationToken);

            game.ScreenData = screenshot ?? _emptyScreenshot;
        }
        catch (Exception ex)
        {
            game.ScreenData = _emptyScreenshot;
            _logger.LogError(ex, "Error downloading screenshot for entry {EntryId}", game.EntryId);
        }
    }

    private async Task LoadFilesAsync(GameViewModel game, CancellationToken cancellationToken)
    {
        List<FileResult> files;

        game.SelectedFile = null;
        game.Files.Clear();

        try
        {
            files = await _zxDbService.GetGameFileLinksAsync(game.EntryId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading files for entry {EntryId}", game.EntryId);
            return;
        }

        game.Files.AddRange(files.Select(f => new NameValuePair<string>(Path.GetFileName(f.FileLink), f.FileLink)));

        if (game.Files.Count > 0)
        {
            game.SelectedFile = game.Files[0].Value;
        }
    }
}

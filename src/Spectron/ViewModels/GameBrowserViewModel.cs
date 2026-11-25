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
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using OldBit.Spectron.Database;
using OldBit.Spectron.Database.Results;
using OldBit.Spectron.Dialogs;
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
    private readonly byte[] _emptyScreenshot;

    private bool _isInitialized;

    public ObservableCollection<GameViewModel> Games { get; } = [];

    public IAsyncRelayCommand SearchCommand { get; }

    public IAsyncRelayCommand LocateDatabaseCommand { get; }

    public IAsyncRelayCommand<TopLevel?> DownloadFileCommand { get; }

    [ObservableProperty]
    public partial GameViewModel? SelectedGame { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsDatabaseAvailable { get; private set; }

    [ObservableProperty]
    public partial string? DatabaseError { get; private set; }

    public GameBrowserViewModel(
        ZxDbService zxDbService,
        ZxDbSettings zxDbSettings,
        IApplicationDataService applicationDataService,
        ILogger<GameBrowserViewModel> logger,
        FileDialogs fileDialogs)
    {
        _zxDbService = zxDbService;
        _zxDbSettings = zxDbSettings;
        _applicationDataService = applicationDataService;
        _logger = logger;
        _fileDialogs = fileDialogs;

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
        _isInitialized = true;

        RefreshAvailability();
    }

    partial void OnSelectedGameChanged(GameViewModel? value) => _ = LoadEntryDataAsync(value);

    internal async Task SetDatabasePathAsync(string databasePath)
    {
        if (!await ZxDbService.IsValidDatabaseAsync(databasePath))
        {
            DatabaseError = "The selected file is not a valid ZXDB database.";
            return;
        }

        _zxDbSettings.DatabasePath = databasePath;
        await _applicationDataService.SaveAsync(new GameBrowserSettings { ZxDbPath = databasePath });

        Games.Clear();
        DatabaseError = null;
        RefreshAvailability();
    }

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

        var uri = ZxDbFileUrlResolver.Resolve(SelectedGame.SelectedFile);

        if (uri is null)
        {
            return;
        }

        await topLevel.Launcher.LaunchUriAsync(new Uri(uri));
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
            var results = await _zxDbService.SearchByTitleAsync(SearchText, cancellationToken: cancellationToken);

            Games.Clear();

            foreach (var result in results)
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
        }
        catch (Exception ex) when (ex is SqliteException or ZxDbNotConfiguredException)
        {
            DatabaseError = $"Unable to read ZXDB database: {ex.Message}";
            RefreshAvailability();
        }
    }

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

        try
        {
            files = await _zxDbService.GetGameFileLinksAsync(game.EntryId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading files for entry {EntryId}", game.EntryId);
            return;
        }

        game.SelectedFile = null;

        game.Files.Clear();
        game.Files.AddRange(files.Select(f => new NameValuePair<string>(Path.GetFileName(f.FileLink), f.FileLink)));

        if (game.Files.Count > 0)
        {
            game.SelectedFile = game.Files[0].Value;
        }
    }
}

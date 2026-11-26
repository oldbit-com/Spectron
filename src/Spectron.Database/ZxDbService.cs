using Microsoft.Extensions.Caching.Memory;
using OldBit.Spectron.Database.Results;

namespace OldBit.Spectron.Database;

public sealed class ZxDbService(
    ZxDbClient zxDbClient,
    ZxDbSettings settings,
    IMemoryCache memoryCache,
    IHttpClientFactory httpClientFactory)
{
    private static readonly TimeSpan ScreenshotCacheDuration = TimeSpan.FromMinutes(10);

    private sealed record ScreenshotCacheKey(int EntryId);

    public bool IsAvailable => zxDbClient.IsAvailable;

    public static Task<bool> IsValidDatabaseAsync(string databasePath, CancellationToken cancellationToken = default)
        => ZxDbClient.IsValidDatabaseAsync(databasePath, cancellationToken);

    public const int DefaultSearchLimit = 500;

    public async Task<GameSearchResults> SearchAsync(
        string text,
        GameSearchField field,
        int limit = DefaultSearchLimit,
        CancellationToken cancellationToken = default)
    {
        // One extra row reveals whether the list was cut off, without a separate count query.
        var games = await zxDbClient.SearchAsync(text, field, limit + 1, cancellationToken);

        var isTruncated = games.Count > limit;

        if (isTruncated)
        {
            games.RemoveRange(limit, games.Count - limit);
        }

        return new GameSearchResults(games, isTruncated);
    }

    public async Task<byte[]?> GetScreenshotAsync(int entryId, CancellationToken cancellationToken = default) =>
        await memoryCache.GetOrCreateAsync(new ScreenshotCacheKey(entryId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = ScreenshotCacheDuration;

            var path = await zxDbClient.GetScreenshotFileAsync(entryId, cancellationToken);

            var url = ZxDbFileUrlResolver.Resolve(path);

            if (url == null)
            {
                return null;
            }

            var client = httpClientFactory.CreateClient();

            return await client.GetByteArrayAsync(url, cancellationToken);
        });

    public Task<List<FileResult>> GetGameFileLinksAsync(int entryId, CancellationToken cancellationToken = default) =>
        zxDbClient.GetGameFileLinksAsync(entryId, cancellationToken);

    public Task<List<AuthorResult>> GetAuthors(int entryId, CancellationToken cancellationToken = default) =>
        zxDbClient.GetAuthors(entryId, cancellationToken);

    /// <summary>
    /// Downloads a ZXDB file into the local download cache, mirroring the remote folder structure,
    /// so it keeps its original name and can be reopened later (recent files, favorites) without network access.
    /// </summary>
    /// <returns>Path to the local copy, or null if the file link cannot be resolved.</returns>
    public async Task<string?> DownloadFileAsync(string fileLink, CancellationToken cancellationToken = default)
    {
        var url = ZxDbFileUrlResolver.Resolve(fileLink);

        if (url == null)
        {
            return null;
        }

        var localPath = GetLocalPath(new Uri(url));

        if (File.Exists(localPath))
        {
            return localPath;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

        var partialPath = $"{localPath}.{Guid.NewGuid():N}.part";

        try
        {
            var client = httpClientFactory.CreateClient();

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using (var fileStream = File.Create(partialPath))
            {
                await response.Content.CopyToAsync(fileStream, cancellationToken);
            }

            File.Move(partialPath, localPath, overwrite: true);
        }
        finally
        {
            File.Delete(partialPath);
        }

        return localPath;
    }

    private string GetLocalPath(Uri uri)
    {
        var root = Path.GetFullPath(settings.DownloadDirectory);

        var segments = new[] { uri.Host }
            .Concat(uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Select(Uri.UnescapeDataString))
            .Select(SanitizeSegment);

        var localPath = Path.GetFullPath(Path.Join([root, ..segments]));

        if (!localPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Invalid file location: {uri}");
        }

        return localPath;
    }

    private static string SanitizeSegment(string segment)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Concat(segment.Select(c => invalidChars.Contains(c) ? '_' : c));

        return sanitized is "." or ".." ? "_" : sanitized;
    }
}

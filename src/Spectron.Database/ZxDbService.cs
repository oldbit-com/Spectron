using Microsoft.Extensions.Caching.Memory;
using OldBit.Spectron.Database.Results;

namespace OldBit.Spectron.Database;

public sealed class ZxDbService(
    ZxDbClient zxDbClient,
    IMemoryCache memoryCache,
    IHttpClientFactory httpClientFactory)
{
    private static readonly TimeSpan ScreenshotCacheDuration = TimeSpan.FromMinutes(10);

    private sealed record ScreenshotCacheKey(int EntryId);

    public bool IsAvailable => zxDbClient.IsAvailable;

    public static Task<bool> IsValidDatabaseAsync(string databasePath, CancellationToken cancellationToken = default)
        => ZxDbClient.IsValidDatabaseAsync(databasePath, cancellationToken);

    public Task<List<GameSearchResult>> SearchByTitleAsync(
        string title,
        int limit = 25,
        CancellationToken cancellationToken = default) => zxDbClient.SearchByTitleAsync(title, limit, cancellationToken);

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
}

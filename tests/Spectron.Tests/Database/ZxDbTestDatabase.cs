using Microsoft.Data.Sqlite;

namespace OldBit.Spectron.Tests.Database;

internal sealed class ZxDbTestDatabase : IDisposable
{
    private readonly string _directory;

    internal string Path { get; }

    private ZxDbTestDatabase(string directory, string path)
    {
        _directory = directory;
        Path = path;
    }

    internal static ZxDbTestDatabase Create(bool isValidSchema = true)
    {
        const string validSchema =
            "CREATE TABLE entries (id INTEGER PRIMARY KEY, title TEXT);" +
            "CREATE TABLE search_by_titles (entry_title TEXT, entry_id INTEGER);" +
            "CREATE TABLE downloads (id INTEGER PRIMARY KEY, entry_id INTEGER, release_seq INTEGER, file_link TEXT, filetype_id INTEGER);";

        var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"spectron-zxdb-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        var path = System.IO.Path.Combine(directory, "zxdb.sqlite");

        using (var connection = new SqliteConnection($"Data Source={path};Pooling=False"))
        {
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = isValidSchema
                ? validSchema
                : "CREATE TABLE something_else (id INTEGER PRIMARY KEY);";
            command.ExecuteNonQuery();
        }

        return new ZxDbTestDatabase(directory, path);
    }

    internal void AddDownload(int id, int entryId, string fileLink, byte fileTypeId, short releaseSeq = 0)
    {
        using var connection = new SqliteConnection($"Data Source={Path};Pooling=False");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO downloads (id, entry_id, release_seq, file_link, filetype_id) " +
            "VALUES ($id, $entryId, $releaseSeq, $fileLink, $fileTypeId)";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$entryId", entryId);
        command.Parameters.AddWithValue("$releaseSeq", releaseSeq);
        command.Parameters.AddWithValue("$fileLink", fileLink);
        command.Parameters.AddWithValue("$fileTypeId", fileTypeId);
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
            // Best effort clean-up.
        }
    }
}

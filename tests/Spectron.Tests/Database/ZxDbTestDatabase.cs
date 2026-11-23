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
        var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"spectron-zxdb-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        var path = System.IO.Path.Combine(directory, "zxdb.sqlite");

        using (var connection = new SqliteConnection($"Data Source={path};Pooling=False"))
        {
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = isValidSchema
                ? "CREATE TABLE entries (id INTEGER PRIMARY KEY, title TEXT); CREATE TABLE search_by_titles (entry_title TEXT, entry_id INTEGER);"
                : "CREATE TABLE something_else (id INTEGER PRIMARY KEY);";
            command.ExecuteNonQuery();
        }

        return new ZxDbTestDatabase(directory, path);
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

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
            "CREATE TABLE entries (id INTEGER PRIMARY KEY, title TEXT, machinetype_id INTEGER, genretype_id INTEGER, availabletype_id TEXT);" +
            "CREATE TABLE search_by_titles (entry_title TEXT, entry_id INTEGER);" +
            "CREATE TABLE downloads (id INTEGER PRIMARY KEY, entry_id INTEGER, release_seq INTEGER, file_link TEXT, filetype_id INTEGER);" +
            "CREATE TABLE machinetypes (id INTEGER PRIMARY KEY, text TEXT);" +
            "CREATE TABLE genretypes (id INTEGER PRIMARY KEY, text TEXT);" +
            "CREATE TABLE availabletypes (id TEXT PRIMARY KEY, text TEXT);" +
            "CREATE TABLE releases (entry_id INTEGER, release_seq INTEGER, release_year INTEGER, PRIMARY KEY (entry_id, release_seq));" +
            "CREATE TABLE labels (id INTEGER PRIMARY KEY, name TEXT);" +
            "CREATE TABLE publishers (entry_id INTEGER, release_seq INTEGER, label_id INTEGER, publisher_seq INTEGER, PRIMARY KEY (entry_id, release_seq, label_id));" +
            "CREATE TABLE search_by_names (label_name TEXT, label_id INTEGER, PRIMARY KEY (label_name, label_id));" +
            "CREATE TABLE search_by_publishers (label_id INTEGER, entry_id INTEGER, PRIMARY KEY (label_id, entry_id));" +
            "CREATE TABLE search_by_authors (label_id INTEGER, entry_id INTEGER, PRIMARY KEY (label_id, entry_id));";

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

    internal void AddEntry(int id, string title, short? year = null, byte? machineTypeId = null)
    {
        Execute(
            "INSERT INTO entries (id, title, machinetype_id) VALUES ($id, $title, $machineTypeId);" +
            "INSERT INTO search_by_titles (entry_title, entry_id) VALUES ($searchTitle, $id);" +
            "INSERT INTO releases (entry_id, release_seq, release_year) VALUES ($id, 0, $year);",
            ("$id", id),
            ("$title", title),
            ("$searchTitle", Normalize(title)),
            ("$year", year),
            ("$machineTypeId", machineTypeId));
    }

    internal void AddLabel(int id, string name) =>
        Execute(
            "INSERT INTO labels (id, name) VALUES ($id, $name);" +
            "INSERT INTO search_by_names (label_name, label_id) VALUES ($searchName, $id);",
            ("$id", id),
            ("$name", name),
            ("$searchName", Normalize(name)));

    internal void AddPublisher(int entryId, int labelId, short releaseSeq = 0)
    {
        if (releaseSeq == 0)
        {
            Execute(
                "INSERT INTO publishers (entry_id, release_seq, label_id, publisher_seq) VALUES ($entryId, 0, $labelId, 1);",
                ("$entryId", entryId),
                ("$labelId", labelId));
        }

        Execute(
            "INSERT OR IGNORE INTO search_by_publishers (label_id, entry_id) VALUES ($labelId, $entryId);",
            ("$entryId", entryId),
            ("$labelId", labelId));
    }

    internal void AddAuthor(int entryId, int labelId) =>
        Execute(
            "INSERT INTO search_by_authors (label_id, entry_id) VALUES ($labelId, $entryId);",
            ("$entryId", entryId),
            ("$labelId", labelId));

    private void Execute(string sql, params (string Name, object? Value)[] parameters)
    {
        using var connection = new SqliteConnection($"Data Source={Path};Pooling=False");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        command.ExecuteNonQuery();
    }

    private static string Normalize(string text) => string.Concat(text.Where(char.IsLetterOrDigit)).ToLowerInvariant();

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

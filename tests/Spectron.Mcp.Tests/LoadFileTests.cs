using System.IO.Compression;
using ModelContextProtocol;
using OldBit.Spectron.Mcp.Tests.Fixtures;
using OldBit.Spectron.Mcp.Tools;

namespace OldBit.Spectron.Mcp.Tests;

public sealed class LoadFileTests : IDisposable
{
    private readonly TestEmulatorHost _host = new();
    private readonly EmulatorTools _tools;
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"spectron-mcp-{Guid.NewGuid():N}");

    public LoadFileTests()
    {
        Directory.CreateDirectory(_folder);
        _tools = new EmulatorTools(new EmulatorController(_host));
    }

    [Fact]
    public async Task LoadFile_ShouldRejectMissingFile()
    {
        await Should.ThrowAsync<McpException>(() => _tools.LoadFile("/no/such/file.tap"));
    }

    [Fact]
    public async Task LoadFile_ShouldRejectUnsupportedFile()
    {
        var path = CreateFile("notes.txt");

        var ex = await Should.ThrowAsync<McpException>(() => _tools.LoadFile(path));

        ex.Message.ShouldBe("Unsupported file type: .txt");
        _host.LoadedFilePath.ShouldBeNull();
    }

    [Fact]
    public async Task LoadFile_ShouldRejectPokeFile()
    {
        var path = CreateFile("game.pok");

        var ex = await Should.ThrowAsync<McpException>(() => _tools.LoadFile(path));

        ex.Message.ShouldContain("POKE files are not supported");
        _host.LoadedFilePath.ShouldBeNull();
    }

    [Fact]
    public async Task LoadFile_ShouldRejectEntryForNonArchive()
    {
        var path = CreateFile("game.tap");

        var ex = await Should.ThrowAsync<McpException>(() => _tools.LoadFile(path, "game.tap"));

        ex.Message.ShouldBe("The entry parameter only applies to zip archives.");
    }

    [Fact]
    public async Task LoadFile_ShouldReportLoadErrorInsteadOfDialog()
    {
        var path = CreateFile("game.sna");
        _host.LoadFileException = new InvalidDataException("Invalid SNA file.");

        var ex = await Should.ThrowAsync<McpException>(() => _tools.LoadFile(path));

        ex.Message.ShouldBe("Failed to load game.sna: Invalid SNA file.");
    }

    [Theory]
    [InlineData("game.tzx", "Inserted tape game.tzx and entered LOAD \"\". Use run_frames to let it load.")]
    [InlineData("game.z80", "Loaded snapshot game.z80.")]
    [InlineData("game.spectron", "Loaded snapshot game.spectron.")]
    [InlineData("game.trd", "Inserted disk game.trd into drive A and enabled the Beta 128 interface.")]
    [InlineData("game.mdr", "Inserted cartridge game.mdr into microdrive 1 and enabled Interface 1.")]
    [InlineData("game.rzx", "Started RZX playback of game.rzx.")]
    public async Task LoadFile_ShouldDescribeWhatWasLoaded(string fileName, string expected)
    {
        var path = CreateFile(fileName);

        var result = await _tools.LoadFile(path);

        result.ShouldBe(expected);
        _host.LoadedFilePath.ShouldBe(path);
        _host.LoadedArchiveEntry.ShouldBeNull();
    }

    [Fact]
    public async Task LoadFile_ShouldLoadOnlySupportedFileInArchive()
    {
        var path = CreateZip("game.zip", "readme.txt", "game.tap");

        var result = await _tools.LoadFile(path);

        result.ShouldBe("Inserted tape game.tap from game.zip and entered LOAD \"\". Use run_frames to let it load.");
        _host.LoadedArchiveEntry.ShouldBe("game.tap");
    }

    [Fact]
    public async Task LoadFile_ShouldRequireEntryWhenArchiveHasSeveralFiles()
    {
        var path = CreateZip("games.zip", "side1.tap", "side2.tap");

        var ex = await Should.ThrowAsync<McpException>(() => _tools.LoadFile(path));

        ex.Message.ShouldBe("games.zip contains 2 supported files, choose one with entry: side1.tap, side2.tap");
        _host.LoadedFilePath.ShouldBeNull();
    }

    [Fact]
    public async Task LoadFile_ShouldLoadChosenArchiveEntry()
    {
        var path = CreateZip("games.zip", "side1.tap", "side2.z80");

        var result = await _tools.LoadFile(path, "SIDE2.Z80");

        result.ShouldBe("Loaded snapshot side2.z80 from games.zip.");
        _host.LoadedArchiveEntry.ShouldBe("side2.z80");
    }

    [Fact]
    public void ResolveArchiveEntry_ShouldMatchFileNameInFolder()
    {
        var path = CreateZip("games.zip", "Games/side1.tap", "Games/side2.tap");

        EmulatorTools.ResolveArchiveEntry(path, "side2.tap").ShouldBe("Games/side2.tap");
        EmulatorTools.ResolveArchiveEntry(path, "games/side1.tap").ShouldBe("Games/side1.tap");
    }

    [Fact]
    public void ResolveArchiveEntry_ShouldRejectAmbiguousFileName()
    {
        var path = CreateZip("games.zip", "A/game.tap", "B/game.tap");

        var ex = Should.Throw<McpException>(() => EmulatorTools.ResolveArchiveEntry(path, "game.tap"));

        ex.Message.ShouldBe("More than one file named game.tap in games.zip, use the full path: A/game.tap, B/game.tap");
    }

    [Fact]
    public void ResolveArchiveEntry_ShouldRejectMissingEntry()
    {
        var path = CreateZip("games.zip", "side1.tap", "side2.tap");

        var ex = Should.Throw<McpException>(() => EmulatorTools.ResolveArchiveEntry(path, "side3.tap"));

        ex.Message.ShouldBe("side3.tap not found in games.zip. Supported files: side1.tap, side2.tap");
    }

    [Fact]
    public void ResolveArchiveEntry_ShouldRejectArchiveWithoutSupportedFiles()
    {
        var path = CreateZip("docs.zip", "readme.txt");

        var ex = Should.Throw<McpException>(() => EmulatorTools.ResolveArchiveEntry(path, null));

        ex.Message.ShouldBe("No supported files found in docs.zip.");
    }

    [Fact]
    public void ResolveArchiveEntry_ShouldRejectCorruptArchive()
    {
        var path = CreateFile("broken.zip");

        var ex = Should.Throw<McpException>(() => EmulatorTools.ResolveArchiveEntry(path, null));

        ex.Message.ShouldStartWith("Failed to read archive broken.zip:");
    }

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    private string CreateFile(string fileName)
    {
        var path = Path.Combine(_folder, fileName);
        File.WriteAllBytes(path, [1, 2, 3]);

        return path;
    }

    private string CreateZip(string fileName, params string[] entries)
    {
        var path = Path.Combine(_folder, fileName);

        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);

        foreach (var entry in entries)
        {
            using var stream = zip.CreateEntry(entry).Open();
            stream.Write([1, 2, 3]);
        }

        return path;
    }
}

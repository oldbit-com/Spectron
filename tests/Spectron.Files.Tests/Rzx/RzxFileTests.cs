using System.Reflection;
using OldBit.Spectron.Files.Rzx;

namespace OldBit.Spectron.Files.Tests.Rzx;

public class RzxFileTests
{
    [Fact]
    public void RzxFile_ShouldLoad()
    {
        using var file = LoadTestFile("test.rzx");
        var rzxFile = RzxFile.Load(file);

        rzxFile.Creator?.CreatorName.ShouldStartWith("Spectaculator");
        rzxFile.Creator?.MajorVersion.ShouldBe(52);
        rzxFile.Creator?.MinorVersion.ShouldBe(371);

        rzxFile.Snapshots.ShouldHaveSingleItem();
        rzxFile.Snapshots[0].Extension.ShouldBe("z80");

        rzxFile.Snapshots[0].Recording.ShouldNotBeNull();
        rzxFile.Snapshots[0].Recording!.Frames.Count.ShouldBe(21381);
    }

    private static FileStream LoadTestFile(string fileName)
    {
        var location = typeof(RzxFileTests).GetTypeInfo().Assembly.Location;
        var dir = Path.GetDirectoryName(location) ?? throw new InvalidOperationException();
        var path =  Path.Combine(dir, "TestFiles", fileName);

        return File.OpenRead(path);
    }
}
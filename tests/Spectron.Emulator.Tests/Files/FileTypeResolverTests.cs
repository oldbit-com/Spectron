using OldBit.Spectron.Emulation.Files;

namespace OldBit.Spectron.Emulator.Tests.Files;

public class FileTypeResolverTests
{
    [Theory]
    [InlineData("/path/test.Tap", FileType.Tap)]
    [InlineData("/path/test.Tzx", FileType.Tzx)]
    [InlineData("/path/test.Sna", FileType.Sna)]
    [InlineData("/path/test.Z80", FileType.Z80)]
    [InlineData("/path/test.Szx", FileType.Szx)]
    [InlineData("/path/test.Zip", FileType.Zip)]
    [InlineData("/path/test.Pok", FileType.Pok)]
    [InlineData("/path/test.Mdr", FileType.Mdr)]
    [InlineData("/path/test.Trd", FileType.Trd)]
    [InlineData("/path/test.Scl", FileType.Scl)]
    [InlineData("/path/test.Spectron", FileType.Spectron)]
    [InlineData("/path/test.txt", FileType.Unsupported)]
    public void FilePath_ShouldResolveFileType(string filePath, FileType expectedFileType)
    {
        var fileType = FileTypeResolver.FromPath(filePath);

        fileType.ShouldBe(expectedFileType);
    }
}
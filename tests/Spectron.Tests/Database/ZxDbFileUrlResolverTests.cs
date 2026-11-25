using OldBit.Spectron.Database;

namespace OldBit.Spectron.Tests.Database;

public class ZxDbFileUrlResolverTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("relative/file.tap")]
    [InlineData("/unknown/file.tap")]
    public void Resolve_ShouldReturnNull_WhenPathNotSupported(string? path) =>
        ZxDbFileUrlResolver.Resolve(path).ShouldBeNull();

    [Theory]
    [InlineData("https://example.com/file.tap")]
    [InlineData("HTTP://example.com/file.tap")]
    public void Resolve_ShouldReturnPath_WhenAlreadyUrl(string path) =>
        ZxDbFileUrlResolver.Resolve(path).ShouldBe(path);

    [Fact]
    public void Resolve_ShouldUseWorldOfSpectrum_ForPubPath() =>
        ZxDbFileUrlResolver.Resolve("/pub/sinclair/screens/load/h/scr/HeadOverHeels.scr")
            .ShouldBe("https://worldofspectrum.net/pub/sinclair/screens/load/h/scr/HeadOverHeels.scr");

    [Fact]
    public void Resolve_ShouldUseSpectrumComputing_ForZxDbPath() =>
        ZxDbFileUrlResolver.Resolve("/zxdb/sinclair/entries/0002259/HeadOverHeels.tzx.zip")
            .ShouldBe("https://spectrumcomputing.co.uk/zxdb/sinclair/entries/0002259/HeadOverHeels.tzx.zip");

    [Fact]
    public void Resolve_ShouldEscapeSegments_ButKeepSlashes() =>
        ZxDbFileUrlResolver.Resolve("/pub/sinclair/games/j/Jet Set Willy (1984)#1.tzx.zip")
            .ShouldBe("https://worldofspectrum.net/pub/sinclair/games/j/Jet%20Set%20Willy%20%281984%29%231.tzx.zip");
}

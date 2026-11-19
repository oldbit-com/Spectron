namespace OldBit.Spectron.Tests;

public class AppVersionTests
{
    [Theory]
    [InlineData("1.0.2+edb499effb92eba74371e56bd0f4f61a5bd63cd0", "1.0.2")]
    [InlineData("1.1.0-beta.1+edb499e", "1.1.0-beta.1")]
    [InlineData("1.0.2", "1.0.2")]
    [InlineData("", "unknown")]
    [InlineData(null, "unknown")]
    public void Parse_StripsSourceRevision(string? informationalVersion, string expected) =>
        AppVersion.Parse(informationalVersion).ShouldBe(expected);
}

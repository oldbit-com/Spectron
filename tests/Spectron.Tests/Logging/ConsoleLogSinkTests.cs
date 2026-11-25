using OldBit.Spectron.Logging;

namespace OldBit.Spectron.Tests.Logging;

public class ConsoleLogSinkTests
{
    [Fact]
    public void FormatMessage_ReplacesNamedPlaceholdersPositionally()
    {
        var message = ConsoleLogSink.FormatMessage(
            "Binding",
            null,
            "An error occurred binding {Property} to {Expression} at {ExpressionErrorPoint}: {Message}",
            ["Text", "Foo.Bar", "Foo", "Could not find property"]);

        message.ShouldBe("[Binding] An error occurred binding 'Text' to 'Foo.Bar' at 'Foo': 'Could not find property'");
    }

    [Fact]
    public void FormatMessage_MissingValues_RendersEmpty()
    {
        var message = ConsoleLogSink.FormatMessage("Layout", null, "{First} and {Second}", ["one"]);

        message.ShouldBe("[Layout] 'one' and ''");
    }

    [Fact]
    public void FormatMessage_EscapedBraces_AreKeptLiteral()
    {
        var message = ConsoleLogSink.FormatMessage("Visual", null, "{{literal}} {Value}", [42]);

        message.ShouldBe("[Visual] {literal} '42'");
    }

    [Fact]
    public void FormatMessage_FormatSpecifier_IsApplied()
    {
        var message = ConsoleLogSink.FormatMessage("Visual", null, "Value {Value:X4}", [255]);

        message.ShouldBe("[Visual] Value '00FF'");
    }

    [Fact]
    public void FormatMessage_Exception_RendersExceptionMessage()
    {
        var message = ConsoleLogSink.FormatMessage("Control", null, "Failed: {Exception}", [new InvalidOperationException("boom")]);

        message.ShouldBe("[Control] Failed: 'boom'");
    }

    [Fact]
    public void FormatMessage_UnterminatedPlaceholder_IsOutputVerbatim()
    {
        var message = ConsoleLogSink.FormatMessage("Control", null, "Broken {Value", ["x"]);

        message.ShouldBe("[Control] Broken {Value");
    }

    [Fact]
    public void FormatMessage_WithSource_AppendsSourceType()
    {
        var source = new object();

        var message = ConsoleLogSink.FormatMessage("Control", source, "Hello", []);

        message.ShouldBe($"[Control] Hello (Object #{source.GetHashCode()})");
    }
}

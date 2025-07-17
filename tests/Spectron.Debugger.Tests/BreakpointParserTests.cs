using OldBit.Spectron.Debugger.Breakpoints;

namespace OldBit.Spectron.Debugger.Tests;

public class BreakpointParserTests
{
    [Theory]
    [InlineData("INVALID==0x1234")]
    [InlineData("A")]
    [InlineData("BC")]
    [InlineData("BC=")]
    [InlineData("BC==")]
    [InlineData("BC==0x")]
    [InlineData("BC=0")]
    public void WhenInvalidCondition_ShouldReturnFalse(string condition)
    {
        var result = BreakpointParser.TryParse(condition, out var breakpoint);

        result.ShouldBeFalse();
        breakpoint.ShouldBeNull();
    }

    [Theory]
    [InlineData("PC==0x1234")]
    [InlineData("PC == $1234")]
    [InlineData("PC == 1234h")]
    [InlineData(" PC== 4660 ")]
    [InlineData("PC== #1234")]
    public void WhenValidCondition_ShouldReturnTrue(string condition)
    {
        var result = BreakpointParser.TryParse(condition, out var breakpoint);

        result.ShouldBeTrue();
        breakpoint.ShouldNotBeNull();
        breakpoint.ShouldBeOfType<RegisterBreakpoint>();

        var registerBreakpoint = (RegisterBreakpoint)breakpoint;

        registerBreakpoint.Register.ShouldBe(Register.PC);
        registerBreakpoint.Value.ShouldBe(0x1234);
    }
}
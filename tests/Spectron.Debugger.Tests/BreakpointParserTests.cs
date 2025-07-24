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
    [InlineData("invalid")]
    [InlineData("0x4000=0x10")]
    [InlineData("0x4000==0x100")]
    [InlineData("0x10000==0")]
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
    public void WhenValidCondition_ShouldParseRegisterBreakpoint(string condition)
    {
        var result = BreakpointParser.TryParse(condition, out var breakpoint);

        result.ShouldBeTrue();
        breakpoint.ShouldNotBeNull();
        breakpoint.ShouldBeOfType<RegisterBreakpoint>();

        var registerBreakpoint = (RegisterBreakpoint)breakpoint;

        registerBreakpoint.Register.ShouldBe(Register.PC);
        registerBreakpoint.Value.ShouldBe((Word)0x1234);
    }

    [Theory]
    [InlineData("16384==0xFF")]
    [InlineData(" 0x4000 ==255")]
    [InlineData(" $4000== #FF ")]
    [InlineData(" $4000 ", true)]
    [InlineData(" 16384 ", true)]
    public void WhenValidCondition_ShouldParseMemoryBreakpoint(string condition, bool isValueNull = false)
    {
        var result = BreakpointParser.TryParse(condition, out var breakpoint);

        result.ShouldBeTrue();
        breakpoint.ShouldNotBeNull();
        breakpoint.ShouldBeOfType<MemoryBreakpoint>();

        var memoryBreakpoint = (MemoryBreakpoint)breakpoint;

        memoryBreakpoint.Address.ShouldBe((Word)0x4000);

        if (isValueNull)
        {
            memoryBreakpoint.Value.ShouldBeNull();
        }
        else
        {
            memoryBreakpoint.Value.ShouldBe((byte)0xFF);
        }
    }
}
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Mcp.Input;
using static OldBit.Spectron.Emulation.Devices.Keyboard.SpectrumKey;

namespace OldBit.Spectron.Mcp.Tests;

public class KeyMapperTests
{
    [Fact]
    public void FromText_ShouldMapLettersDigitsAndSymbols()
    {
        var keys = KeyMapper.FromText("aZ1\"\n ");

        keys.ShouldBe([[A], [CapsShift, Z], [D1], [SymbolShift, P], [Enter], [Space]]);
    }

    [Fact]
    public void FromText_ShouldTreatCrLfAsSingleEnter()
    {
        KeyMapper.FromText("\r\n").ShouldBe([[Enter]]);
    }

    [Fact]
    public void FromText_ShouldRejectCharactersNotOnKeyboard()
    {
        Should.Throw<ArgumentException>(() => KeyMapper.FromText("{"));
    }

    [Theory]
    [InlineData("enter", new[] { Enter })]
    [InlineData("SYM+P", new[] { SymbolShift, P })]
    [InlineData("caps + 5", new[] { CapsShift, D5 })]
    [InlineData("BREAK", new[] { CapsShift, Space })]
    [InlineData("CAPS+BREAK", new[] { CapsShift, Space })]
    [InlineData("q", new[] { Q })]
    public void ParseCombination_ShouldParseKeyNames(string combination, SpectrumKey[] expected)
    {
        KeyMapper.ParseCombination(combination).ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("F1")]
    [InlineData("SYM+?")]
    public void ParseCombination_ShouldRejectUnknownKeys(string combination)
    {
        Should.Throw<ArgumentException>(() => KeyMapper.ParseCombination(combination));
    }
}

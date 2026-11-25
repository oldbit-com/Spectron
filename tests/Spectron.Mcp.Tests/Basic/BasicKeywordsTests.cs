using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Mcp.Basic;
using static OldBit.Spectron.Emulation.Devices.Keyboard.SpectrumKey;

namespace OldBit.Spectron.Mcp.Tests.Basic;

public class BasicKeywordsTests
{
    [Fact]
    public void All_ShouldContainEveryTokenOnce()
    {
        BasicKeywords.All.Count.ShouldBe(91);
        BasicKeywords.All.Select(keyword => keyword.Token).ShouldBe(Enumerable.Range(0xA5, 91).Select(token => (byte)token));
        BasicKeywords.All.Select(keyword => keyword.Name).ShouldBeUnique();
    }

    [Theory]
    [InlineData(0xA5, "RND", "Extended", T)]
    [InlineData(0xA6, "INKEY$", "Extended", N)]
    [InlineData(0xB2, "SIN", "Extended", Q)]
    [InlineData(0xC7, "<=", "SymbolShift", Q)]
    [InlineData(0xC9, "<>", "SymbolShift", W)]
    [InlineData(0xCB, "THEN", "SymbolShift", G)]
    [InlineData(0xCC, "TO", "SymbolShift", F)]
    [InlineData(0xCE, "DEF FN", "ExtendedSymbolShift", D1)]
    [InlineData(0xD3, "OPEN #", "ExtendedSymbolShift", D4)]
    [InlineData(0xD8, "CIRCLE", "ExtendedSymbolShift", H)]
    [InlineData(0xD9, "INK", "ExtendedSymbolShift", X)]
    [InlineData(0xDA, "PAPER", "ExtendedSymbolShift", C)]
    [InlineData(0xE2, "STOP", "SymbolShift", A)]
    [InlineData(0xE7, "BORDER", "KMode", B)]
    [InlineData(0xEA, "REM", "KMode", E)]
    [InlineData(0xEC, "GO TO", "KMode", G)]
    [InlineData(0xED, "GO SUB", "KMode", H)]
    [InlineData(0xF5, "PRINT", "KMode", P)]
    [InlineData(0xFF, "COPY", "KMode", Z)]
    public void Get_ShouldReturnNameAndKey(int token, string name, string entry, SpectrumKey key)
    {
        var keyword = BasicKeywords.Get((byte)token);

        keyword.Name.ShouldBe(name);
        keyword.Entry.ShouldBe(Enum.Parse<KeywordEntry>(entry));
        keyword.Key.ShouldBe(key);
    }

    [Fact]
    public void Keys_ShouldIncludeSymbolShift()
    {
        BasicKeywords.Get(0xD8).Keys.ShouldBe([SymbolShift, H]);
        BasicKeywords.Get(0xB2).Keys.ShouldBe([Q]);
        BasicKeywords.Get(0xF5).Keys.ShouldBe([P]);
    }

    [Fact]
    public void ExtendedCharacters_ShouldContainCharactersNotOnKeyMapper()
    {
        BasicKeywords.ExtendedCharacters.ShouldBe(new Dictionary<char, SpectrumKey>
        {
            ['~'] = A, ['\\'] = D, ['{'] = F, ['}'] = G, ['©'] = P, ['|'] = S, [']'] = U, ['['] = Y
        }, ignoreOrder: true);
    }

    [Fact]
    public void Detokenize_ShouldSpellOutKeywords()
    {
        BasicKeywords.Detokenize([0x31, 0x30, 0xF5, 0x22, 0x61, 0x60, 0x22, 0x3B, 0xCC]).ShouldBe("10 PRINT \"a£\"; TO");
    }
}

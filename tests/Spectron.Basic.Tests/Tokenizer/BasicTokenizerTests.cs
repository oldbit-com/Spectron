using OldBit.Spectron.Basic.Reader;
using OldBit.Spectron.Basic.Tokenizer;
using Spectron.Basic.Tests.Fixtures;

namespace Spectron.Basic.Tests.Tokenizer;

public class BasicTokenizerTests
{
    private readonly TestMemory _memory = new();
    private readonly BasicTokenizer _tokenizer = new();
    private readonly BasicMemoryReader _memoryReader;

    public BasicTokenizerTests()
    {
        _memory.Write(0x5C53, 0xCB);
        _memory.Write(0x5C54, 0x5C);

        _memoryReader = new BasicMemoryReader(_memory);
    }

    [Theory]
    [MemberData(nameof(StatementData))]
    public void Tokenizer_ShouldTokenizeStatement(string instruction, Token[] expectedTokens)
    {
        var data = ParseHexString(instruction);
        _memory.Write(0x5CCB, data);

        var lines = _memoryReader.ReadAllLines();
        var tokens = _tokenizer.Tokenize(lines.Single());

        tokens.ShouldBeEquivalentTo(expectedTokens.ToList());
    }

    public static TheoryData<string, Token[]> StatementData => new()
    {
        // {
        //     // 10 REM Test program
        //     "00 0A 0E 00 EA 54 65 73 74 20 70 72 6F 67 72 61 6D 0D 80 80",
        //     [Token.LineNumber(10), Token.Keyword(0xEA), Token.String("Test program"), Token.EndOfLine]
        // },
        // {
        //     // 10 PRINT "Hello World"
        //     "00 0A 0F 00 F5 22 48 65 6C 6C 6F 20 57 6F 72 6C 64 22 0D 80 80",
        //     [Token.LineNumber(10), Token.Keyword(0xF5), Token.String("\"Hello World\""), Token.EndOfLine]
        // },
        // {
        //     // 20 PRINT "a""b"
        //     "00 14 08 00 F5 22 61 22 22 62 22 0D 80 80",
        //     [Token.LineNumber(20), Token.Keyword(0xF5), Token.String("\"a\"\"b\""), Token.EndOfLine]
        // },
        // {
        //     // 30 PRINT """a"
        //     "00 1E 07 00 F5 22 22 22 61 22 0D 80 80",
        //     [Token.LineNumber(30), Token.Keyword(0xF5), Token.String("\"\"\"a\""), Token.EndOfLine]
        // },
        // {
        //     // 40 PRINT "say ""hello"" to ""world"""
        //     "00 28 1E 00 F5 22 73 61 79 20 22 22 68 65 6C 6C 6F 22 22 20 74 6F 20 22 22 77 6F 72 6C 64 22 22 22 0D 80 80",
        //     [Token.LineNumber(40), Token.Keyword(0xF5), Token.String("\"say \"\"hello\"\" to \"\"world\"\"\""), Token.EndOfLine]
        // },
        {
            // 50 PRINT "a""b";123
            "00 32 12 00 F5 22 61 22 22 62 22 3B 31 32 33 0E 00 00 7B 00 00 0D 80 0D 80 80",
            [Token.LineNumber(50), Token.Keyword(0xF5), Token.String("\"a\"\"b\""), Token.Separator]
        }
    };

    private static byte[] ParseHexString(string hex) => hex
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(b => Convert.ToByte(b, 16))
        .ToArray();
}
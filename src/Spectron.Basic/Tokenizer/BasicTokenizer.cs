using OldBit.Spectron.Basic.Reader;

namespace OldBit.Spectron.Basic.Tokenizer;

public class BasicTokenizer
{
    private const string OneQuote = "\"";
    private const string TwoQuotes = "\"\"";

    private const byte CarriageReturn = 0x0D;
    private const byte Number = 0x0E;
    private const byte Ink = 0x10;
    private const byte Paper = 0x11;
    private const byte Flash = 0x12;
    private const byte Bright = 0x13;
    private const byte Inverse = 0x14;
    private const byte Over = 0x15;
    private const byte At = 0x16;
    private const byte Tab = 0x17;
    private const byte DoubleQuote = 0x22;
    private const byte Separator = 0x3A;

    public List<Token> Tokenize(BasicLine line)
    {
        var tokens = new List<Token>
        {
            new(TokenType.LineNumber, line.LineNumber)
        };

        var inString = false;
        var inRem = false;
        var s = string.Empty;

        var i = 0;

        while (i < line.Data.Length)
        {
            // End of line
            if (line.Data[i] == CarriageReturn)
            {
                if (inRem)
                {
                    tokens.Add(Token.String(s));
                }

                tokens.Add(new Token(TokenType.EndOfLine, CarriageReturn));
                break;
            }

            // REM comments
            if (inRem)
            {
                s += (char)line.Data[i];
                i += 1;
                continue;
            }

            // Strings
            if (line.Data[i] == DoubleQuote)
            {
                if (!inString)
                {
                    s += OneQuote;
                    inString = true;
                }
                else if (i + 1 < line.Data.Length && line.Data[i + 1] == DoubleQuote)
                {
                    s += TwoQuotes;
                    i += 1;
                }
                else
                {
                    s += OneQuote;
                    tokens.Add(Token.String(s));
                    inString = false;
                }

                i += 1;
                continue;
            }

            if (inString)
            {
                s += (char)line.Data[i];
                i += 1;
                continue;
            }

            // Control characters
            if (line.Data[i] >= Ink && line.Data[i] <= Tab)
            {
                var tokenType = GetControlTokenType(line.Data[i]);

                if (tokenType is TokenType.Tab or TokenType.At)
                {
                    if (i + 2 >= line.Data.Length)
                    {
                        tokens.Add(new Token(tokenType, line.Data[(i + 1) .. (i + 3)]));
                    }

                    i += 3;
                }
                else
                {
                    if (i + 1 < line.Data.Length)
                    {
                        tokens.Add(new Token(tokenType, line.Data[i + 1]));
                    }

                    i += 2;
                }

                continue;
            }

            // Numbers
            if (line.Data[i] == Number)
            {
                // TODO:
            }

            // Variable table
            // // Numeric variable
            // if ((line.Data[i] & 0xF0) == 0x60)
            // {
            //
            // }
            //
            // // Numeric variable with a multi-character name
            // if ((line.Data[i] & 0xF0) == 0xA0)
            // {
            //
            // }
            //
            // // String variable
            // if ((line.Data[i] & 0xF0) == 0x40)
            // {
            //
            // }
            //
            // // Numeric array
            // if ((line.Data[i] & 0xF0) == 0x80)
            // {
            //
            // }
            //
            // // Character array
            // if ((line.Data[i] & 0xF0) == 0xC0)
            // {
            //
            // }
            //
            // // Control variable of a FOR-NEXT loop
            // if ((line.Data[i] & 0xF0) == 0xE0)
            // {
            //
            // }

            // Keywords
            if (line.Data[i] >= 0xA5)
            {
                tokens.Add(new Token(TokenType.Keyword, line.Data[i]));

                // REM
                if (line.Data[i] == 0xEA)
                {
                    inRem = true;
                    s = string.Empty;
                }

                i += 1;
                continue;
            }

            if (line.Data[i] == Separator)
            {
                i += 1;
                tokens.Add(Token.Separator);
                continue;
            }

            s += (char)line.Data[i];
            i += 1;
        }

        return tokens;
    }

    private static TokenType GetControlTokenType(byte code) => code switch
    {
        Ink => TokenType.Ink,
        Paper => TokenType.Paper,
        Flash => TokenType.Flash,
        Bright => TokenType.Bright,
        Inverse => TokenType.Inverse,
        Over => TokenType.Over,
        At => TokenType.At,
        Tab => TokenType.Tab,
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
    };
}
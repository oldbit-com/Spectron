using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using OldBit.Spectron.Debugger.ViewModels;

namespace OldBit.Spectron.Debugger.Breakpoints;

public static class BreakpointParser
{
    private static readonly string[] ValidRegisters =
    [
        "A", "B", "C", "D", "E", "H", "L", "IXH", "IXL", "IYH", "IYL",
        "AF", "BC", "DE", "HL", "SP", "PC", "IX", "IY"
    ];

    public static bool TryParse(string? condition, [NotNullWhen(true)] out Breakpoint? breakpoint)
    {
        if (TryParse(condition, out RegisterBreakpoint? registerBreakpoint))
        {
            breakpoint = registerBreakpoint;
            return true;
        }

        if (TryParse(condition, out MemoryBreakpoint? memoryBreakpoint))
        {
            breakpoint = memoryBreakpoint;
            return true;
        }

        breakpoint = null;
        return false;
    }

    private static bool TryParse(string? condition, [NotNullWhen(true)] out RegisterBreakpoint? breakpoint)
    {
        breakpoint = null;

        if (string.IsNullOrWhiteSpace(condition))
        {
            return false;
        }

        var items = condition.Split("==");

        if (items.Length != 2)
        {
            return false;
        }

        if (!TryParseRegister(items[0].Trim(), out var register))
        {
            return false;
        }

        if (!TryParseWord(items[1].Trim(), out var value))
        {
            return false;
        }

        if (!Is16BitRegister(register) && value > 0xFF)
        {
            return false;
        }

        breakpoint = new RegisterBreakpoint(Enum.Parse<Register>(register, true), value)
        {
            Condition = condition
        };

        return true;
    }

    private static bool TryParse(string? condition, [NotNullWhen(true)] out MemoryBreakpoint? breakpoint)
    {
        breakpoint = null;

        if (string.IsNullOrWhiteSpace(condition))
        {
            return false;
        }

        var items = condition.Split("==");

        if (items.Length != 1 && items.Length != 2)
        {
            return false;
        }

        if (!TryParseWord(items[0].Trim(), out var address))
        {
            return false;
        }

        if (items.Length == 1)
        {
            breakpoint = new MemoryBreakpoint(address)
            {
                Condition = condition
            };
        }
        else
        {
            if (!TryParseByte(items[1].Trim(), out var value))
            {
                return false;
            }

            breakpoint = new MemoryBreakpoint(address, value)
            {
                Condition = condition
            };
        }

        return true;
    }

    private static bool TryParseRegister(string value, [NotNullWhen(true)] out string? register)
    {
        register = ValidRegisters.FirstOrDefault(r => r.Equals(value, StringComparison.OrdinalIgnoreCase));

        return register != null;
    }

    private static bool TryParseWord(string value, out Word result)
    {
        result = 0;

        if (!int.TryParse(value, out var intValue) && !TryParseHex(value, out intValue))
        {
            return false;
        }

        if (intValue is < 0 or > 0xFFFF)
        {
            return false;
        }

        result = (Word)intValue;

        return true;
    }

    private static bool TryParseByte(string value, out byte result)
    {
        result = 0;

        if (!TryParseWord(value, out var word))
        {
            return false;
        }

        if (word > 0xFF)
        {
            return false;
        }

        result = (byte)word;

        return true;
    }

    private static bool TryParseHex(string hex, out int value)
    {
        if (hex.StartsWith('$') || hex.StartsWith('#'))
        {
            if (int.TryParse(hex.AsSpan(1), NumberStyles.HexNumber, null, out value))
            {
                return true;
            }
        }

        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(hex.AsSpan(2), NumberStyles.HexNumber, null, out value))
            {
                return true;
            }
        }

        if (hex.EndsWith("h", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(hex.AsSpan(0, hex.Length - 1), NumberStyles.HexNumber, null, out value))
            {
                return true;
            }
        }

        value = 0;

        return false;
    }

    private static bool Is16BitRegister(string register) => register.Length == 2;
}
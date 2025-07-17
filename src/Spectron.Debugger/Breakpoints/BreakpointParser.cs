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

        if (TryParse(condition, out RegisterBreakpoint? memoryBreakpoint))
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

        var register = ParseRegister(items[0].Trim());

        if (register == null)
        {
            return false;
        }

        var address = ParseAddress(items[1].Trim());

        if (address == null)
        {
            return false;
        }

        if (Is16BitRegister(register))
        {
            if (address is < 0 or > 0xFFFF)
            {
                return false;
            }
        }
        else
        {
            if (address is < 0 or > 0xFF)
            {
                return false;
            }
        }

        breakpoint = new RegisterBreakpoint(Enum.Parse<Register>(register, true), (Word)address.Value);

        return true;
    }

    private static bool TryParse(string? condition, [NotNullWhen(true)] out MemoryBreakpoint? breakpoint)
    {
        breakpoint = null;

        return false;
    }

    private static string? ParseRegister(string value) =>
        ValidRegisters.FirstOrDefault(r => r.Equals(value, StringComparison.OrdinalIgnoreCase));

    private static int? ParseAddress(string value)
    {
        if (int.TryParse(value, out var address) || TryParseHex(value, out address))
        {
            return address;
        }

        return null;
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
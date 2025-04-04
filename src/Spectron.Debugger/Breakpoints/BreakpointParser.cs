using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OldBit.Spectron.Debugger.Breakpoints;

public static partial class BreakpointParser
{
    private static readonly string[] ValidRegisters =
    [
        "A", "B", "C", "D", "E", "H", "L", "IXH", "IXL", "IYH", "IYL",
        "AF", "BC", "DE", "HL", "SP", "PC", "IX", "IY"
    ];

    public static bool TryParseCondition(string? condition, [NotNullWhen(true)]out (Register Register, int Address)? breakpoint)
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

        var register = GetRegister(items[0].Trim());

        if (register == null)
        {
            return false;
        }

        var address = GetAddress(items[1].Trim());

        if (address == null)
        {
            return false;
        }

        if (Is16BitRegister(register))
        {
            if (address < 0 || address > 0xFFFF)
                return false;
        }
        else
        {
            if (address is < 0 or > 0xFF)
            {
                return false;
            }
        }

        breakpoint = (Enum.Parse<Register>(register, true), address.Value);

        return true;
    }

    private static string? GetRegister(string value) =>
        ValidRegisters.FirstOrDefault(r => r.Equals(value, StringComparison.OrdinalIgnoreCase));

    private static int? GetAddress(string value)
    {
        if (int.TryParse(value, out var address))
        {
            return address;
        }

        if (TryParseHex(value, out address))
        {
            return address;
        }

        return null;
    }

    private static bool TryParseHex(string hex, out int value)
    {
        if (hex.StartsWith('$'))
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
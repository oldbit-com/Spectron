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

    public static bool TryParseCondition(string? condition, [NotNullWhen(true)]out Breakpoint? breakpoint)
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

        breakpoint = new Breakpoint(register, address.Value);

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

        var match = HexValuePattern().Match(value);

        if (!match.Success)
        {
            return null;
        }

        var hex = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
        if (int.TryParse(hex, NumberStyles.HexNumber, null, out address))
        {
            return address;
        }

        return null;
    }

    private static bool Is16BitRegister(string register) => register.Length == 2;

    [GeneratedRegex(@"(?i)(?:\$(?=[0-9A-F])|0x)([0-9A-F]+)|([0-9A-F]+)(?=h)")]
    private static partial Regex HexValuePattern();
}
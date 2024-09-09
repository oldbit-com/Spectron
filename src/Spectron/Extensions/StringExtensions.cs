using System.Text.RegularExpressions;

namespace OldBit.Spectron.Extensions;

public static partial class StringExtensions
{
    public static string ToKebabCase(this string s)
    {
        return string.IsNullOrEmpty(s) ? s : KebabCaseRegex().Replace(s, "-$1").ToLower();
    }

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex KebabCaseRegex();
}
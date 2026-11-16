using System.Text.RegularExpressions;

namespace OldBit.Spectron.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex KebabCaseRegex();

    extension(string s)
    {
        public string ToKebabCase() => string.IsNullOrEmpty(s) ? s : KebabCaseRegex().Replace(s, "-$1").ToLowerInvariant();

        public string Truncate(int maxLength) => s.Length <= maxLength ? s : s[..maxLength];
    }
}
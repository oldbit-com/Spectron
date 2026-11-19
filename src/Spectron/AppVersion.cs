using System.Reflection;

namespace OldBit.Spectron;

internal static class AppVersion
{
    /// <summary>
    /// Application version, e.g. "1.0.2". Set at build time from the release tag.
    /// </summary>
    internal static string Current { get; } = Parse(
        typeof(AppVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

    internal static string Parse(string? informationalVersion)
    {
        if (string.IsNullOrWhiteSpace(informationalVersion))
        {
            return "unknown";
        }

        // Strip the "+<commit>" source revision metadata appended by the SDK
        var plusIndex = informationalVersion.IndexOf('+');

        return plusIndex >= 0 ? informationalVersion[..plusIndex] : informationalVersion;
    }
}

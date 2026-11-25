namespace OldBit.Spectron.Database;

public static class ZxDbFileUrlResolver
{
    private const string WorldOfSpectrum = "https://worldofspectrum.net/";

    private const string SpectrumComputing = "https://spectrumcomputing.co.uk/";

    public static string? Resolve(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        if (path.StartsWith("/pub/", StringComparison.Ordinal))
        {
            return WorldOfSpectrum + Encode(path.TrimStart('/'));
        }

        if (path.StartsWith("/zxdb/", StringComparison.Ordinal))
        {
            return SpectrumComputing + Encode(path.TrimStart('/'));
        }

        return null;
    }

    private static string Encode(string relative) =>
        string.Join('/', relative.Split('/').Select(Uri.EscapeDataString));
}

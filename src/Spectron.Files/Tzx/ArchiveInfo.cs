namespace OldBit.Spectron.Files.Tzx;

/// <summary>
/// Represents a utility class for handling archive information properties in the TZX file format.
/// </summary>
public static class ArchiveInfo
{
    /// <summary>
    /// Represents the identifier for the "Title" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Title = 0x00;

    /// <summary>
    /// Represents the identifier for the "Publisher" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Publisher = 0x01;

    /// <summary>
    /// Represents the identifier for the "Author" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Author = 0x02;

    /// <summary>
    /// Represents the identifier for the "Year" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Year = 0x03;

    /// <summary>
    /// Represents the identifier for the "Language" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Language = 0x04;

    /// <summary>
    /// Represents the identifier for the "Type" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Type = 0x05;

    /// <summary>
    /// Represents the identifier for the "Price" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Price = 0x06;

    /// <summary>
    /// Represents the identifier for the "Protection" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Protection = 0x07;

    /// <summary>
    /// Represents the identifier for the "Origin" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Origin = 0x08;

    /// <summary>
    /// Represents the identifier for the "Comments" property in the archive information section of a TZX file.
    /// </summary>
    public const byte Comments = 0xFF;

    /// <summary>
    /// Gets the display name of the specified archive info property.
    /// </summary>
    /// <param name="id">The id of archive info property.</param>
    /// <returns>The display name of the archive info property.</returns>
    public static string GetName(byte id) => id switch
    {
        Title => "Title",
        Publisher => "Publisher",
        Author => "Author",
        Year => "Year",
        Language => "Language",
        Type => "Type",
        Price => "Price",
        Protection => "Protection",
        Origin => "Origin",
        Comments => "Comments",
        _ => string.Empty
    };
}

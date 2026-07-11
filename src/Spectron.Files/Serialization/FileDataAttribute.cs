namespace OldBit.Spectron.Files.Serialization;

/// <summary>
/// Attribute used by the block serializer.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal class FileDataAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the value that specifies the order in which the property should be serialized.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the size in bytes where it cannot be inferred from the data type.
    /// </summary>
    public int Size { get; set; }
}